using LinqToDB;
using System.Linq;
using LinqToDB.Common;
using Nop.Data;
using Nop.Plugin.Swift.Api.Domain.Shapes;
using NUglify.Helpers;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Swift.Api.Services
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
                throw new ArgumentNullException(nameof(shapes));

            _shapeRepository.Insert(shapes);

            IList<Shape> createdShapes = _shapeRepository.Table.ToList();

            foreach (Shape shape in shapes)
            {
                Shape createdShape = createdShapes.Single(cs => cs.Name == shape.Name);
                shape.Atttributes.ForEach(sa => sa.ShapeId = createdShape.Id);
                _shapeAttributeRepository.Insert(shape.Atttributes);
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
