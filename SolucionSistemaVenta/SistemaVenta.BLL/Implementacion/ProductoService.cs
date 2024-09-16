using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class ProductoService : IProductoService
    { 


        private readonly IGenericRepository<Producto> _repositorio;
        private readonly IFireBaseService _fireBaseServicio;

        public ProductoService(IGenericRepository<Producto> repositorio,
            IFireBaseService fireBaseServicio)
        {
            _repositorio = repositorio;
            _fireBaseServicio = fireBaseServicio;

        }

        public async Task<List<Producto>> Lista()
        {
            //Lógica para este método
            IQueryable<Producto> query = await _repositorio.Consultar();
            return query.Include(c => c.IdCategoriaNavigation).ToList();


        }
        public async Task<Producto> Crear(Producto entidad, Stream imagen = null, string NombreImagen = "")
        {
            Producto producto_existe = await _repositorio.Obtener(p => p.CodigoBarra == entidad.CodigoBarra);

            if(producto_existe != null)
                throw new TaskCanceledException("El código de barra ya está registrado");

            try
            {
                entidad.NombreImagen = NombreImagen;
                if (imagen != null) {
                    string urlImage = await _fireBaseServicio.SubirStorage(imagen, "carpeta_producto", NombreImagen);
                    entidad.UrlImagen = urlImage;
                
                }

                Producto producto_creado = await _repositorio.Crear(entidad);

                if (producto_creado.IdProducto == 0) 
                    throw new TaskCanceledException("El producto no se pudo crear");
                
                IQueryable<Producto> query = await _repositorio.Consultar(p => p.IdProducto == producto_creado.IdProducto);

                producto_creado =  query.Include(c => c.IdCategoriaNavigation).First();

                return producto_creado;
            }
            catch (Exception ex) {

                throw;
            }

        }

        public async Task<Producto> Editar(Producto entidad, Stream imagen = null, string NombreImagen = "")
        {
            //se valida que al momento de editar que ese codigo de barra no ha sido utilizado dentro de otro producto que ya existe en nustra tabla
            Producto producto_existe = await _repositorio.Obtener(p => p.CodigoBarra == entidad.CodigoBarra && p.IdProducto != entidad.IdProducto);

            if(producto_existe != null)
                throw new TaskCanceledException("El código de barra ya está asignado a otro producto");

            try
            {
                IQueryable<Producto> queryProducto = await _repositorio.Consultar(p => p.IdProducto == entidad.IdProducto);

                Producto producto_para_editar = queryProducto.First();

                producto_para_editar.CodigoBarra = entidad.CodigoBarra;
                producto_para_editar.Marca = entidad.Marca;
                producto_para_editar.Descripcion = entidad.Descripcion;
                producto_para_editar.IdCategoria = entidad.IdCategoria;
                producto_para_editar.Stock = entidad.Stock;
                producto_para_editar.Precio = entidad.Precio;
                producto_para_editar.EsActivo = entidad.EsActivo;

                //con esta condicion se asegura de que apesar que no tenga un nombre de imagen se va a crear
                if (producto_para_editar.NombreImagen == "") { 
                    producto_para_editar.NombreImagen = NombreImagen;
                }

                //validacion para poder actulaizar la imagen del producto
                if (imagen != null) { //si realmente existe una imagen para poder actualziar
                    string urlImagen = await _fireBaseServicio.SubirStorage(imagen, "carpeta_producto", producto_para_editar.NombreImagen);
                    producto_para_editar.UrlImagen = urlImagen;
                }

                bool respuesta = await _repositorio.Editar(producto_para_editar);

                if(!respuesta)
                    throw new TaskCanceledException("Error al editar el producto");


                Producto producto_editado = queryProducto.Include(c => c.IdCategoriaNavigation).First();

                return producto_editado;

            }
            catch {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idProducto)
        {
            try
            {
                Producto producto_encontrado = await _repositorio.Obtener(p => p.IdProducto == idProducto);

                if(producto_encontrado == null)
                    throw new TaskCanceledException("No se encontró al producto");

                string nombreImagen = producto_encontrado.NombreImagen;

                bool respuesta = await _repositorio.Eliminar(producto_encontrado);

                //Si la respuesta es true se eliminó al producto de nuestra tabla por lo tanto también desamos eliminar la imagen del producto de nuestro storage de firebase
                if (respuesta)
                    await _fireBaseServicio.EliminarStorage("carpeta_producto", nombreImagen);

                return true;

            }
            catch {
                throw;
            }
        }

     
    }
}
