using LinqToDB;
using System.Linq;
using LinqToDB.Common;
using Nop.Data;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class ShapeService : IShapeService
    {
        private readonly IRepository<Shape> _shapeRepository;
        private readonly IRepository<ShapeAttribute> _shapeAttributeRepository;

        public ShapeService(
            IRepository<Shape> shapeRepository,
            IRepository<ShapeAttribute> shapeAttributeRepository)
        {
            _shapeRepository = shapeRepository;
            _shapeAttributeRepository = shapeAttributeRepository;
        }

        public async Task InsertShapesAsync(List<Shape> shapes)
        {
            if (shapes.IsNullOrEmpty())
            {
                return;
            }

          await  _shapeRepository.InsertAsync(shapes);

            // insert attributes and subcategory
            foreach (Shape shape in shapes)
            {
                if (shape.Atttributes != null && shape.Atttributes.Count > 0)
                {
                    shape.Atttributes.ForEach(sa => sa.ShapeId = shape.Id);

                    // Model needs to be edited
                   await _shapeAttributeRepository.InsertAsync(await shape.Atttributes.ToListAsync());
                }

                if (shape.SubCategories != null && shape.SubCategories.Count > 0)
                {
                    shape.SubCategories.ForEach(sc => { sc.ParentId = shape.Id; sc.SawOption = shape.SawOption; });
                    await InsertShapesAsync((List<Shape>)shape.SubCategories);
                }
            }
        }

        public async Task<IList<Shape>> GetShapesAsync(bool parentsOnly = false)
        {
            var shapes = await  _shapeRepository.Table.Where(s => s.ParentId == null || s.ParentId == 0).ToListAsync();

            foreach (Shape shape in shapes)
            {
                if (shape.ParentId == 0 || shape.ParentId == null)
                {
                    shape.SubCategories = await _shapeRepository.Table.Where(s => s.ParentId.Value == shape.Id).ToListAsync();
                    shape.Atttributes = await _shapeAttributeRepository.Table.Where(sa => sa.ShapeId == shape.Id).ToListAsync();
                }
                else if (!parentsOnly)
                {
                    shape.Parent = await _shapeRepository.Table.FirstOrDefaultAsync(s => s.Id == shape.ParentId);
                    if (shape.Parent != null)
                    {
                        shape.Parent.Atttributes = await _shapeAttributeRepository.Table.Where(sa => sa.ShapeId == shape.Parent.Id).ToListAsync();
                    }
                }

            }

            return shapes;
        }

        public async Task<Shape> GetShapeByIdAsync(int id)
        {
            var shape = await _shapeRepository.Table.FirstOrDefaultAsync(s => s.Id == id);

            if (shape != null)
            {
                if (shape.ParentId == 0 || shape.ParentId == null)
                {
                    shape.SubCategories = await _shapeRepository.Table.Where(s => s.ParentId.Value == shape.Id).ToListAsync();
                    shape.Atttributes = await _shapeAttributeRepository.Table.Where(sa => sa.ShapeId == shape.Id).ToListAsync();
                }
                else
                {
                    shape.Parent = await _shapeRepository.Table.FirstOrDefaultAsync(s => s.Id == shape.ParentId);
                    if (shape.Parent != null)
                    {
                        shape.Parent.Atttributes = await _shapeAttributeRepository.Table.Where(sa => sa.ShapeId == shape.Parent.Id).ToListAsync();
                        shape.Atttributes = shape.Parent.Atttributes;
                    }
                }

            }

            return shape;
        }

        public async Task DeleteShapesAsync()
        {
           await _shapeAttributeRepository.Table.DeleteAsync();
           await _shapeRepository.Table.DeleteAsync();
        }
    }
}
