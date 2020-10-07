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

            //IList<Shape> createdShapes = _shapeRepository.Table.ToList();

            foreach (Shape shape in shapes)
            {
                //Shape createdShape = createdShapes.Single(cs => cs.Name == shape.Name);
                if (shape.Atttributes != null && shape.Atttributes.Count > 0)
                {
                    shape.Atttributes.ForEach(sa => sa.ShapeId = shape.Id);
                    _shapeAttributeRepository.Insert(shape.Atttributes);
                }

                if (shape.SubCategories != null && shape.SubCategories.Count > 0)
                {
                    InsertShapes((List<Shape>)shape.SubCategories);
                }
            }
        }

        public IList<Shape> GetShapes()
        {
            var shapes = _shapeRepository.Table.ToList();

            foreach (Shape shape in shapes) {
                shape.Atttributes = _shapeAttributeRepository.Table.Where(s => s.ShapeId == shape.Id).ToList();
            }

            return shapes;
        }

        public void DeleteShapes()
        {
            _shapeAttributeRepository.Table.Delete();
            _shapeRepository.Table.Delete();
        }
    }
}
