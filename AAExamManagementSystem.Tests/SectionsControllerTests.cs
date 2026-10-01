using AAExamManagementSystem.Controllers;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AAExamManagementSystem.Tests;

public class SectionsControllerTests
{
    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<Section, SectionDto>();
                cfg.CreateMap<SectionCreateUpdateDto, Section>();
            },
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        return configuration.CreateMapper();
    }

    [Fact]
    public async Task GetAll_ReturnsAllSections()
    {
        var repository = new Mock<IGenericRepository<Section>>();
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Section { Id = 1, Name = "Section A" },
            new Section { Id = 2, Name = "Section B" },
        });

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<SectionDto>>(okResult.Value);
        Assert.Equal(2, dtos.Count());
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsSection()
    {
        var repository = new Mock<IGenericRepository<Section>>();
        repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Section { Id = 1, Name = "Section A" });

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SectionDto>(okResult.Value);
        Assert.Equal("Section A", dto.Name);
    }

    [Fact]
    public async Task GetById_WithMissingId_ReturnsNotFound()
    {
        var repository = new Mock<IGenericRepository<Section>>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<object>())).ReturnsAsync((Section?)null);

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_AddsSectionAndReturnsCreatedAtAction()
    {
        var repository = new Mock<IGenericRepository<Section>>();

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.Create(new SectionCreateUpdateDto { Name = "New Section" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<SectionDto>(created.Value);
        Assert.Equal("New Section", dto.Name);
        repository.Verify(r => r.AddAsync(It.Is<Section>(s => s.Name == "New Section")), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_WithExistingId_UpdatesSectionAndReturnsNoContent()
    {
        var section = new Section { Id = 1, Name = "Old Name" };
        var repository = new Mock<IGenericRepository<Section>>();
        repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(section);

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.Update(1, new SectionCreateUpdateDto { Name = "New Name" });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("New Name", section.Name);
        repository.Verify(r => r.Update(section), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_WithMissingId_ReturnsNotFound()
    {
        var repository = new Mock<IGenericRepository<Section>>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<object>())).ReturnsAsync((Section?)null);

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.Update(99, new SectionCreateUpdateDto { Name = "New Name" });

        Assert.IsType<NotFoundResult>(result);
        repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Delete_WithExistingId_SoftDeletesSectionAndReturnsNoContent()
    {
        var section = new Section { Id = 1, Name = "Section A", IsActive = true };
        var repository = new Mock<IGenericRepository<Section>>();
        repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(section);

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
        Assert.False(section.IsActive);
        repository.Verify(r => r.Update(section), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_WithMissingId_ReturnsNotFound()
    {
        var repository = new Mock<IGenericRepository<Section>>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<object>())).ReturnsAsync((Section?)null);

        var controller = new SectionsController(repository.Object, CreateMapper());

        var result = await controller.Delete(99);

        Assert.IsType<NotFoundResult>(result);
        repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
