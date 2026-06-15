using ClassService.DTOs.Classes;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using Moq;
using Xunit;

namespace ClassService.Tests;

public class ClassServiceTests
{
    private readonly Mock<IClassRepository> _mockRepo;
    private readonly ClassService.Services.ClassService _service;

    public ClassServiceTests()
    {
        _mockRepo = new Mock<IClassRepository>();
        _service = new ClassService.Services.ClassService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllClasses()
    {
        // Arrange
        var classes = new List<Class>
        {
            new Class { Id = Guid.NewGuid(), Name = "Class A", GradeLevel = 10, SchoolYear = "2025-2026" },
            new Class { Id = Guid.NewGuid(), Name = "Class B", GradeLevel = 11, SchoolYear = "2025-2026" }
        };
        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(classes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Name == "Class A");
        Assert.Contains(result, r => r.Name == "Class B");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnClass_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Class { Id = id, Name = "Class A", GradeLevel = 10, SchoolYear = "2025-2026" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Class A", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync((Class?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(id));
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAndSaveClass()
    {
        // Arrange
        var dto = new CreateClassDto { Name = "New Class", GradeLevel = 10, SchoolYear = "2025-2026" };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.GradeLevel, result.GradeLevel);
        Assert.Equal(dto.SchoolYear, result.SchoolYear);

        _mockRepo.Verify(repo => repo.AddAsync(It.Is<Class>(c => c.Name == dto.Name)), Times.Once);
        _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndSaveClass_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Class { Id = id, Name = "Old Name", GradeLevel = 10, SchoolYear = "2025-2026" };
        var dto = new UpdateClassDto { Name = "New Name", GradeLevel = 11, SchoolYear = "2026-2027" };

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(entity);

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("New Name", result.Name);
        Assert.Equal(11, result.GradeLevel);
        Assert.Equal("2026-2027", result.SchoolYear);

        _mockRepo.Verify(repo => repo.Update(It.Is<Class>(c => c.Id == id && c.Name == "New Name")), Times.Once);
        _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateClassDto { Name = "New Name", GradeLevel = 11, SchoolYear = "2026-2027" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync((Class?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAndSaveClass_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Class { Id = id, Name = "Class A", GradeLevel = 10, SchoolYear = "2025-2026" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(entity);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);

        _mockRepo.Verify(repo => repo.Delete(entity), Times.Once);
        _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync((Class?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(id));
    }
}
