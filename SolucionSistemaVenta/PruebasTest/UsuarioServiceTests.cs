using Moq;
using SistemaVenta.BLL.Implementacion;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

public class UsuarioServiceTests
{
    // Test para obtener credenciales exitosamente
    [Fact]
    public async Task ObtenerPorCredenciales_LoginCorrecto()
    {
        // Arrange
        var mockRepositorio = new Mock<IGenericRepository<Usuario>>();
        var mockUtilidadesService = new Mock<IUtilidadesService>();
        var mockFireBaseService = new Mock<IFireBaseService>();
        var mockCorreoService = new Mock<ICorreoService>();

        var usuarioValido = new Usuario
        {
            IdUsuario = 6, // Actualizado con el ID correcto
            Correo = "vargassalcedoa33@gmail.com", // Actualizado con el correo correcto
            Clave = "c1bd7bf4d1d2d439c60b62a7527840ebc232f18769eca0ee60dad913ad071499" // Clave encriptada correcta
        };

        // Configuramos los mocks
        mockRepositorio.Setup(r => r.Obtener(It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>()))
            .ReturnsAsync(usuarioValido);

        mockUtilidadesService.Setup(u => u.ConvertirSha256("a84852")).Returns("c1bd7bf4d1d2d439c60b62a7527840ebc232f18769eca0ee60dad913ad071499");

        var usuarioService = new UsuarioService(
            mockRepositorio.Object,
            mockFireBaseService.Object,
            mockUtilidadesService.Object,
            mockCorreoService.Object);

        // Act
        var resultado = await usuarioService.ObtenerPorCredenciales("vargassalcedoa33@gmail.com", "a84852");

        // Assert
        Xunit.Assert.NotNull(resultado);
        Xunit.Assert.Equal(6, resultado.IdUsuario);
    }

   

    // Test para obtener credenciales incorrectas
    [Fact]
    public async Task ObtenerPorCredenciales_LoginIncorrecto()
    {
        // Arrange
        var mockRepositorio = new Mock<IGenericRepository<Usuario>>();
        var mockUtilidadesService = new Mock<IUtilidadesService>();
        var mockFireBaseService = new Mock<IFireBaseService>();
        var mockCorreoService = new Mock<ICorreoService>();

        // Simulamos que no existe un usuario con esas credenciales
        mockRepositorio.Setup(r => r.Obtener(It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>()))
            .ReturnsAsync((Usuario)null);

        mockUtilidadesService.Setup(u => u.ConvertirSha256(It.IsAny<string>())).Returns("c1bd7bf4d1d2d439c60b62a7527840ebc232f18769eca0ee60dad913ad071499");

        var usuarioService = new UsuarioService(
            mockRepositorio.Object,
            mockFireBaseService.Object,
            mockUtilidadesService.Object,
            mockCorreoService.Object);

        // Act
        var resultado = await usuarioService.ObtenerPorCredenciales("vargassalcedoa33@gmail.com", "claveIncorrecta");

        // Assert
        Xunit.Assert.Null(resultado);
    }

    // Test para iniciar sesión correctamente y acceder a la vista
    [Fact]
    public async Task IniciarSesionYAccederVista_PanelAdmin()
    {
        // Arrange
        var mockUsuarioRepositorio = new Mock<IGenericRepository<Usuario>>();
        var mockRolRepositorio = new Mock<IGenericRepository<Rol>>();
        var mockUtilidadesService = new Mock<IUtilidadesService>();
        var mockFireBaseService = new Mock<IFireBaseService>();
        var mockCorreoService = new Mock<ICorreoService>();

        // Datos de prueba
        var rolAdmin = new Rol { IdRol = 1, Descripcion = "Admin" };
        var usuarioValido = new Usuario
        {
            IdUsuario = 6,
            Correo = "vargassalcedoa33@gmail.com",
            Clave = "c1bd7bf4d1d2d439c60b62a7527840ebc232f18769eca0ee60dad913ad071499", // Clave encriptada correcta
            IdRol = 1, // Rol Admin
            IdRolNavigation = rolAdmin // Asignamos el rol directamente en la entidad de usuario
        };

        // Lista de roles para prueba
        var rolList = new List<Rol> { rolAdmin }.AsQueryable();

        // Configuración de los mocks
        mockUsuarioRepositorio.Setup(r => r.Obtener(It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>()))
            .ReturnsAsync(usuarioValido);

        mockUtilidadesService.Setup(u => u.ConvertirSha256(It.IsAny<string>())).Returns(usuarioValido.Clave);

        mockRolRepositorio.Setup(r => r.Consultar(It.IsAny<System.Linq.Expressions.Expression<Func<Rol, bool>>>()))
            .ReturnsAsync(rolList);

        var usuarioService = new UsuarioService(
            mockUsuarioRepositorio.Object,
            mockFireBaseService.Object,
            mockUtilidadesService.Object,
            mockCorreoService.Object);

        // Act: Iniciar sesión
        var usuario = await usuarioService.ObtenerPorCredenciales("vargassalcedoa33@gmail.com", "a84852");

        // Assert: Verificar que el usuario ha sido autenticado y tiene el rol correcto para acceder al panel
        Xunit.Assert.NotNull(usuario);
        Xunit.Assert.Equal(6, usuario.IdUsuario);
        Xunit.Assert.Equal(1, usuario.IdRol); // Verifica que el usuario tiene el rol Admin
    }
}
