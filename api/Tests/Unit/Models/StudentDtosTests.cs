using TherapyDocs.Api.Models.DTOs;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Models;

public class StudentDtosTests
{
    [Fact]
    public void CreateStudentRequest_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var request = new CreateStudentRequest();

        // Assert
        Assert.Equal(string.Empty, request.FirstName);
        Assert.Equal(string.Empty, request.LastName);
        Assert.Null(request.DateOfBirth);
        Assert.Null(request.Gender);
        Assert.Equal(Guid.Empty, request.TherapistId);
        Assert.Null(request.StudentId);
        Assert.Null(request.SchoolName);
        Assert.Null(request.GradeLevel);
        Assert.Null(request.PrimaryDiagnosis);
        Assert.Null(request.SecondaryDiagnoses);
        Assert.Null(request.Goals);
        Assert.Null(request.Notes);
    }

    [Fact]
    public void CreateStudentRequest_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var therapistId = Guid.NewGuid();
        var dateOfBirth = new DateTime(2010, 1, 1);

        // Act
        var request = new CreateStudentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = dateOfBirth,
            Gender = "Male",
            TherapistId = therapistId,
            StudentId = "STU001",
            SchoolName = "Elementary",
            GradeLevel = "3rd",
            PrimaryDiagnosis = "ADHD",
            SecondaryDiagnoses = "Anxiety",
            Goals = "Improve focus",
            Notes = "Positive reinforcement"
        };

        // Assert
        Assert.Equal("John", request.FirstName);
        Assert.Equal("Doe", request.LastName);
        Assert.Equal(dateOfBirth, request.DateOfBirth);
        Assert.Equal("Male", request.Gender);
        Assert.Equal(therapistId, request.TherapistId);
        Assert.Equal("STU001", request.StudentId);
        Assert.Equal("Elementary", request.SchoolName);
        Assert.Equal("3rd", request.GradeLevel);
        Assert.Equal("ADHD", request.PrimaryDiagnosis);
        Assert.Equal("Anxiety", request.SecondaryDiagnoses);
        Assert.Equal("Improve focus", request.Goals);
        Assert.Equal("Positive reinforcement", request.Notes);
    }

    [Fact]
    public void UpdateStudentRequest_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var request = new UpdateStudentRequest();

        // Assert
        Assert.Equal(Guid.Empty, request.Id);
        Assert.Equal(string.Empty, request.FirstName);
        Assert.Equal(string.Empty, request.LastName);
        Assert.Null(request.DateOfBirth);
        Assert.Null(request.Gender);
        Assert.Null(request.StudentId);
        Assert.Null(request.SchoolName);
        Assert.Null(request.GradeLevel);
        Assert.Null(request.PrimaryDiagnosis);
        Assert.Null(request.SecondaryDiagnoses);
        Assert.Null(request.Goals);
        Assert.Null(request.Notes);
        Assert.True(request.IsActive);
    }

    [Fact]
    public void UpdateStudentRequest_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dateOfBirth = new DateTime(2010, 1, 1);

        // Act
        var request = new UpdateStudentRequest
        {
            Id = id,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = dateOfBirth,
            Gender = "Female",
            StudentId = "STU002",
            SchoolName = "High School",
            GradeLevel = "10th",
            PrimaryDiagnosis = "Autism",
            SecondaryDiagnoses = "Sensory Processing",
            Goals = "Social skills",
            Notes = "Visual supports helpful",
            IsActive = false
        };

        // Assert
        Assert.Equal(id, request.Id);
        Assert.Equal("Jane", request.FirstName);
        Assert.Equal("Smith", request.LastName);
        Assert.Equal(dateOfBirth, request.DateOfBirth);
        Assert.Equal("Female", request.Gender);
        Assert.Equal("STU002", request.StudentId);
        Assert.Equal("High School", request.SchoolName);
        Assert.Equal("10th", request.GradeLevel);
        Assert.Equal("Autism", request.PrimaryDiagnosis);
        Assert.Equal("Sensory Processing", request.SecondaryDiagnoses);
        Assert.Equal("Social skills", request.Goals);
        Assert.Equal("Visual supports helpful", request.Notes);
        Assert.False(request.IsActive);
    }

    [Fact]
    public void StudentResponse_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var response = new StudentResponse();

        // Assert
        Assert.False(response.Success);
        Assert.Equal(string.Empty, response.Message);
        Assert.Null(response.Student);
    }

    [Fact]
    public void StudentResponse_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var studentDto = new StudentDto { FirstName = "Test" };

        // Act
        var response = new StudentResponse
        {
            Success = true,
            Message = "Operation successful",
            Student = studentDto
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Operation successful", response.Message);
        Assert.Equal(studentDto, response.Student);
    }

    [Fact]
    public void StudentsListResponse_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var response = new StudentsListResponse();

        // Assert
        Assert.False(response.Success);
        Assert.Equal(string.Empty, response.Message);
        Assert.NotNull(response.Students);
        Assert.Empty(response.Students);
        Assert.Equal(0, response.TotalCount);
    }

    [Fact]
    public void StudentsListResponse_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var students = new List<StudentDto>
        {
            new() { FirstName = "John" },
            new() { FirstName = "Jane" }
        };

        // Act
        var response = new StudentsListResponse
        {
            Success = true,
            Message = "Students retrieved successfully",
            Students = students,
            TotalCount = 2
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Students retrieved successfully", response.Message);
        Assert.Equal(students, response.Students);
        Assert.Equal(2, response.TotalCount);
    }

    [Fact]
    public void StudentDto_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var dto = new StudentDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.FirstName);
        Assert.Equal(string.Empty, dto.LastName);
        Assert.Null(dto.DateOfBirth);
        Assert.Null(dto.Gender);
        Assert.Equal(Guid.Empty, dto.TherapistId);
        Assert.Null(dto.StudentId);
        Assert.Null(dto.SchoolName);
        Assert.Null(dto.GradeLevel);
        Assert.Null(dto.PrimaryDiagnosis);
        Assert.Null(dto.SecondaryDiagnoses);
        Assert.Null(dto.Goals);
        Assert.Null(dto.Notes);
        Assert.False(dto.IsActive);
        Assert.Equal(DateTime.MinValue, dto.CreatedAt);
        Assert.Equal(DateTime.MinValue, dto.UpdatedAt);
        Assert.Null(dto.Therapist);
    }

    [Fact]
    public void StudentDto_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var therapistId = Guid.NewGuid();
        var dateOfBirth = new DateTime(2010, 5, 15);
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var updatedAt = DateTime.UtcNow;
        var therapistDto = new UserDto { FirstName = "Dr. Smith" };

        // Act
        var dto = new StudentDto
        {
            Id = id,
            FirstName = "Michael",
            LastName = "Johnson",
            DateOfBirth = dateOfBirth,
            Gender = "Male",
            TherapistId = therapistId,
            StudentId = "STU003",
            SchoolName = "Middle School",
            GradeLevel = "7th",
            PrimaryDiagnosis = "Learning Disability",
            SecondaryDiagnoses = "ADHD",
            Goals = "Reading comprehension",
            Notes = "Responds well to structured environment",
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Therapist = therapistDto
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("Michael", dto.FirstName);
        Assert.Equal("Johnson", dto.LastName);
        Assert.Equal(dateOfBirth, dto.DateOfBirth);
        Assert.Equal("Male", dto.Gender);
        Assert.Equal(therapistId, dto.TherapistId);
        Assert.Equal("STU003", dto.StudentId);
        Assert.Equal("Middle School", dto.SchoolName);
        Assert.Equal("7th", dto.GradeLevel);
        Assert.Equal("Learning Disability", dto.PrimaryDiagnosis);
        Assert.Equal("ADHD", dto.SecondaryDiagnoses);
        Assert.Equal("Reading comprehension", dto.Goals);
        Assert.Equal("Responds well to structured environment", dto.Notes);
        Assert.True(dto.IsActive);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal(updatedAt, dto.UpdatedAt);
        Assert.Equal(therapistDto, dto.Therapist);
    }

    [Fact]
    public void StudentSummaryDto_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var dto = new StudentSummaryDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.FirstName);
        Assert.Equal(string.Empty, dto.LastName);
        Assert.Null(dto.StudentId);
        Assert.Null(dto.SchoolName);
        Assert.Null(dto.GradeLevel);
        Assert.False(dto.IsActive);
    }

    [Fact]
    public void StudentSummaryDto_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new StudentSummaryDto
        {
            Id = id,
            FirstName = "Sarah",
            LastName = "Williams",
            StudentId = "STU004",
            SchoolName = "Elementary School",
            GradeLevel = "5th",
            IsActive = true
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("Sarah", dto.FirstName);
        Assert.Equal("Williams", dto.LastName);
        Assert.Equal("STU004", dto.StudentId);
        Assert.Equal("Elementary School", dto.SchoolName);
        Assert.Equal("5th", dto.GradeLevel);
        Assert.True(dto.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Special chars: <>\"'&")]
    [InlineData("Unicode: 测试 🎯")]
    public void CreateStudentRequest_StringProperties_ShouldHandleSpecialValues(string value)
    {
        // Act
        var request = new CreateStudentRequest
        {
            FirstName = value,
            LastName = value,
            StudentId = value,
            SchoolName = value,
            GradeLevel = value,
            PrimaryDiagnosis = value,
            SecondaryDiagnoses = value,
            Goals = value,
            Notes = value
        };

        // Assert
        Assert.Equal(value, request.FirstName);
        Assert.Equal(value, request.LastName);
        Assert.Equal(value, request.StudentId);
        Assert.Equal(value, request.SchoolName);
        Assert.Equal(value, request.GradeLevel);
        Assert.Equal(value, request.PrimaryDiagnosis);
        Assert.Equal(value, request.SecondaryDiagnoses);
        Assert.Equal(value, request.Goals);
        Assert.Equal(value, request.Notes);
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Other")]
    [InlineData("Non-binary")]
    [InlineData("Prefer not to say")]
    [InlineData("")]
    [InlineData(null)]
    public void CreateStudentRequest_Gender_ShouldAcceptVariousValues(string? gender)
    {
        // Act
        var request = new CreateStudentRequest { Gender = gender };

        // Assert
        Assert.Equal(gender, request.Gender);
    }

    [Fact]
    public void UpdateStudentRequest_IsActive_ShouldDefaultToTrue()
    {
        // Act
        var request = new UpdateStudentRequest();

        // Assert
        Assert.True(request.IsActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateStudentRequest_IsActive_ShouldAcceptBothValues(bool isActive)
    {
        // Act
        var request = new UpdateStudentRequest { IsActive = isActive };

        // Assert
        Assert.Equal(isActive, request.IsActive);
    }

    [Fact]
    public void StudentResponse_WithNullStudent_ShouldBeValid()
    {
        // Act
        var response = new StudentResponse
        {
            Success = false,
            Message = "Student not found",
            Student = null
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Student not found", response.Message);
        Assert.Null(response.Student);
    }

    [Fact]
    public void StudentsListResponse_WithEmptyList_ShouldBeValid()
    {
        // Act
        var response = new StudentsListResponse
        {
            Success = true,
            Message = "No students found",
            Students = new List<StudentDto>(),
            TotalCount = 0
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("No students found", response.Message);
        Assert.Empty(response.Students);
        Assert.Equal(0, response.TotalCount);
    }

    [Fact]
    public void StudentsListResponse_TotalCountMismatch_ShouldStillWork()
    {
        // This test shows that TotalCount is independent of the actual list count
        // which might be intentional for pagination scenarios

        // Arrange
        var students = new List<StudentDto> { new() { FirstName = "Test" } };

        // Act
        var response = new StudentsListResponse
        {
            Success = true,
            Message = "Partial results",
            Students = students,
            TotalCount = 100 // Different from actual list count
        };

        // Assert
        Assert.True(response.Success);
        Assert.Single(response.Students);
        Assert.Equal(100, response.TotalCount);
    }

    [Theory]
    [InlineData(2000, 1, 1)]
    [InlineData(2010, 6, 15)]
    [InlineData(2020, 12, 31)]
    public void StudentDto_DateOfBirth_ShouldAcceptValidDates(int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var dto = new StudentDto { DateOfBirth = date };

        // Assert
        Assert.Equal(date, dto.DateOfBirth);
    }

    [Fact]
    public void AllDtos_WithNullOptionalProperties_ShouldBeValid()
    {
        // Act
        var createRequest = new CreateStudentRequest
        {
            FirstName = "Required",
            LastName = "Required",
            TherapistId = Guid.NewGuid()
        };
        
        var updateRequest = new UpdateStudentRequest
        {
            Id = Guid.NewGuid(),
            FirstName = "Required",
            LastName = "Required"
        };
        
        var dto = new StudentDto
        {
            FirstName = "Required",
            LastName = "Required"
        };
        
        var summaryDto = new StudentSummaryDto
        {
            FirstName = "Required",
            LastName = "Required"
        };

        // Assert - No exceptions should be thrown
        Assert.NotNull(createRequest);
        Assert.NotNull(updateRequest);
        Assert.NotNull(dto);
        Assert.NotNull(summaryDto);
    }
}