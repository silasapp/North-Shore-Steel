using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using NSS.Plugin.Misc.SwiftApi.DTOs.Shapes;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
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
