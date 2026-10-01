using AAExamManagementSystem.Controllers;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AAExamManagementSystem.Tests;

public class DepartmentsControllerTests
{
    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<Department, DepartmentDto>();
                cfg.CreateMap<DepartmentCreateUpdateDto, Department>();
            },
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        return configuration.CreateMapper();
    }

    [Fact]
    public async Task GetAll_ReturnsAllDepartments()
    {
        var repository = new Mock<IGenericRepository<Department>>();
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Department { Id = "dept-1", Name = "Department A" },
            new Department { Id = "dept-2", Name = "Department B" },
        });

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value);
        Assert.Equal(2, dtos.Count());
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsDepartment()
    {
        var repository = new Mock<IGenericRepository<Department>>();
        repository.Setup(r => r.GetByIdAsync("dept-1")).ReturnsAsync(new Department { Id = "dept-1", Name = "Department A" });

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.GetById("dept-1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<DepartmentDto>(okResult.Value);
        Assert.Equal("Department A", dto.Name);
    }

    [Fact]
    public async Task GetById_WithMissingId_ReturnsNotFound()
    {
        var repository = new Mock<IGenericRepository<Department>>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<object>())).ReturnsAsync((Department?)null);

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.GetById("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_AddsDepartmentAndReturnsCreatedAtAction()
    {
        var repository = new Mock<IGenericRepository<Department>>();

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.Create(new DepartmentCreateUpdateDto { Name = "New Department", IsActive = true });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<DepartmentDto>(created.Value);
        Assert.Equal("New Department", dto.Name);
        repository.Verify(r => r.AddAsync(It.Is<Department>(d => d.Name == "New Department")), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_WithExistingId_UpdatesDepartmentAndReturnsNoContent()
    {
        var department = new Department { Id = "dept-1", Name = "Old Name", IsActive = true };
        var repository = new Mock<IGenericRepository<Department>>();
        repository.Setup(r => r.GetByIdAsync("dept-1")).ReturnsAsync(department);

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.Update("dept-1", new DepartmentCreateUpdateDto { Name = "New Name", IsActive = false });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("New Name", department.Name);
        Assert.False(department.IsActive);
        repository.Verify(r => r.Update(department), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_WithMissingId_ReturnsNotFound()
    {
        var repository = new Mock<IGenericRepository<Department>>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<object>())).ReturnsAsync((Department?)null);

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.Update("missing", new DepartmentCreateUpdateDto { Name = "New Name" });

        Assert.IsType<NotFoundResult>(result);
        repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Delete_WithExistingId_SoftDeletesDepartmentAndReturnsNoContent()
    {
        var department = new Department { Id = "dept-1", Name = "Department A", IsActive = true };
        var repository = new Mock<IGenericRepository<Department>>();
        repository.Setup(r => r.GetByIdAsync("dept-1")).ReturnsAsync(department);

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.Delete("dept-1");

        Assert.IsType<NoContentResult>(result);
        Assert.False(department.IsActive);
        repository.Verify(r => r.Update(department), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_WithMissingId_ReturnsNotFound()
    {
        var repository = new Mock<IGenericRepository<Department>>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<object>())).ReturnsAsync((Department?)null);

        var controller = new DepartmentsController(repository.Object, CreateMapper());

        var result = await controller.Delete("missing");

        Assert.IsType<NotFoundResult>(result);
        repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
