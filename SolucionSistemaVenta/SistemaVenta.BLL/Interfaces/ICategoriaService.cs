using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    public interface ICategoriaService
    {
        // creación de los métodos que va a contener este servicio
        Task<List<Categoria>> Lista(); //creamos una tarea y primero se va a devolver una lista de categorias
        Task<Categoria> Crear(Categoria entidad); //para crear una categoría
        Task<Categoria> Editar(Categoria entidad); //para editar la categoría
        Task<bool> Eliminar(int idCategoria); // para eliminar la categoría

        //Luego se tiene que crear la implementación para este servicio
    }
}
