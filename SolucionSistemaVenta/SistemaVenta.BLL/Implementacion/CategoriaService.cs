using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Referencias necesarias
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IGenericRepository<Categoria> _repositorio;

        public CategoriaService(IGenericRepository<Categoria> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<Categoria>> Lista()
        {
            IQueryable<Categoria> query = await _repositorio.Consultar();
            return query.ToList();
        }
        public async Task<Categoria> Crear(Categoria entidad)
        {
            try
            {
                Categoria categoria_creada = await _repositorio.Crear(entidad);
                if(categoria_creada.IdCategoria == 0) //no se creó la categoría entonces se devuelve una tarea que se ha cancelado por una excepcion
                    throw new TaskCanceledException("Error. No se pudo crear la categoria");

                return categoria_creada;
            }
            catch {
                throw;
            }
        }

        public async Task<Categoria> Editar(Categoria entidad)
        {
            try
            {
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == entidad.IdCategoria);
                categoria_encontrada.Descripcion = entidad.Descripcion;
                categoria_encontrada.EsActivo = entidad.EsActivo;

                //almacenamos la respuesta del método de editar
                bool respuesta = await _repositorio.Editar(categoria_encontrada);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar la categoria");

                return categoria_encontrada;
            }
            catch {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idCategoria)
        {
            try
            {
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == idCategoria);

                if(categoria_encontrada == null)
                    throw new TaskCanceledException("No se encontró a la categoría");

                bool respuesta = await _repositorio.Eliminar(categoria_encontrada);

                return respuesta;
            }
            catch {
                throw;
            }
        }

      
    }
}
