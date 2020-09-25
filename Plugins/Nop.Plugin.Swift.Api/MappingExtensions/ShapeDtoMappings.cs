using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Swift.Api.Domain.Shapes;
using Nop.Plugin.Swift.Api.DTOs.Shapes;
using System.Collections.Generic;

namespace Nop.Plugin.Swift.Api.MappingExtensions
{
    public static class ShapeDtoMappings
    {
        public static Shape ToEntity(this ShapeDto shapeDto)
        {
            return shapeDto.MapTo<ShapeDto, Shape>();
        }

        public static ShapeDto ToDto(this Shape shape)
        {
            return shape.MapTo<Shape, ShapeDto>();
        }

        public static List<ShapeDto> ToDto(this List<Shape> shapes)
        {
            return shapes.MapTo< List<Shape>, List<ShapeDto>>();
        }

        public static List<Shape> ToEntity(this List<ShapeDto> shapesDto)
        {
            return shapesDto.MapTo< List<ShapeDto>, List<Shape>>();
        }
    }
}
