using LinqToDB;
using System.Linq;
using LinqToDB.Common;
using Nop.Data;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using NUglify.Helpers;
using System;
using System.Collections.Generic;

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

        public void InsertShapes(List<Shape> shapes)
        {
            if (shapes.IsNullOrEmpty())
            {
                return;
            }

            _shapeRepository.Insert(shapes);

            // insert attributes and subcategory
            foreach (Shape shape in shapes)
            {
                if (shape.Atttributes != null && shape.Atttributes.Count > 0)
                {
                    shape.Atttributes.ForEach(sa => sa.ShapeId = shape.Id);
                    _shapeAttributeRepository.Insert(shape.Atttributes);
                }

                if (shape.SubCategories != null && shape.SubCategories.Count > 0)
                {
                    shape.SubCategories.ForEach(sc => { sc.ParentId = shape.Id; sc.SawOption = shape.SawOption; });
                    InsertShapes((List<Shape>)shape.SubCategories);
                }
            }
        }

        public IList<Shape> GetShapes()
        {
            var shapes = _shapeRepository.Table.Where(s => s.ParentId == null || s.ParentId == 0).ToList();

            foreach (Shape shape in shapes) {
                shape.SubCategories = _shapeRepository.Table.Where(s => s.ParentId.Value == shape.Id).ToList();
                shape.Atttributes = _shapeAttributeRepository.Table.Where(s => s.ShapeId == shape.Id).ToList();
            }

            return shapes;
        }

        public Shape GetShapeById(int id)
        {
            var shape = _shapeRepository.Table.FirstOrDefault(s => s.Id == id);

            if(shape != null)
            {
                if (shape.ParentId == 0 || shape.ParentId == null)
                {
                    shape.SubCategories = _shapeRepository.Table.Where(s => s.ParentId.Value == shape.Id).ToList();
                    shape.Atttributes = _shapeAttributeRepository.Table.Where(sa => sa.ShapeId == shape.Id).ToList();
                }
                else
                {
                    shape.Parent = _shapeRepository.Table.FirstOrDefault(s => s.Id == shape.ParentId);
                    if (shape.Parent != null)
                    {
                        shape.Parent.Atttributes = _shapeAttributeRepository.Table.Where(sa => sa.ShapeId == shape.Parent.Id).ToList();
                    }
                }

            }

            return shape;
        }

        public void DeleteShapes()
        {
            _shapeAttributeRepository.Table.Delete();
            _shapeRepository.Table.Delete();
        }
    }
}
