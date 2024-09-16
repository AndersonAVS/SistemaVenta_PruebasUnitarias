using System.Reflection;
using System.Runtime.Loader;
// este archivo nos va a permitir trabajar con extensiones externas de nuestro proyecto
namespace SistemaVenta.AplicacionWeb.Utilidades.Extensiones
{
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return LoadUnmanagedDllFromPath(unmanagedDllName);
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }
    }
}

// para poder utilizar esta configuración se tiene que hacer en Program.cs
