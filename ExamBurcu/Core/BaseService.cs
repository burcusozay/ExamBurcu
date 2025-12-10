using AutoMapper;

namespace VaccineExam.Core
{
    public class BaseService<TEntity, TDto>
    {
        protected readonly IMapper _mapper;

        protected BaseService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public virtual TEntity MapToEntity(TDto dto)
        {
            return _mapper.Map<TEntity>(dto);
        }

        public virtual TDto MapToDto(TEntity entity)
        {
            return _mapper.Map<TDto>(entity);
        }

        public virtual List<TDto> MapToDtoList(List<TEntity> entities)
        {
            return _mapper.Map<List<TDto>>(entities);
        }
    }
}
