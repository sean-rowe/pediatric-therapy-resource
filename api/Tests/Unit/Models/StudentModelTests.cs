using System.ComponentModel.DataAnnotations;
using TherapyDocs.Api.Models;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Models;

public class StudentModelTests
{
    [Fact]
    public void Student_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var student = new Student();

        // Assert
        Assert.Equal(Guid.Empty, student.Id);
        Assert.Equal(string.Empty, student.FirstName);
        Assert.Equal(string.Empty, student.LastName);
        Assert.Null(student.DateOfBirth);
        Assert.Null(student.Gender);
        Assert.Equal(Guid.Empty, student.TherapistId);
        Assert.Null(student.StudentId);
        Assert.Null(student.SchoolName);
        Assert.Null(student.GradeLevel);
        Assert.Null(student.PrimaryDiagnosis);
        Assert.Null(student.SecondaryDiagnoses);
        Assert.Null(student.Goals);
        Assert.Null(student.Notes);
        Assert.True(student.IsActive);
        Assert.True(student.CreatedAt <= DateTime.UtcNow);
        Assert.True(student.UpdatedAt <= DateTime.UtcNow);
        Assert.Null(student.Therapist);
    }

    [Fact]
    public void Student_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var therapistId = Guid.NewGuid();
        var dateOfBirth = new DateTime(2010, 1, 1);
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var student = new Student
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = dateOfBirth,
            Gender = "Male",
            TherapistId = therapistId,
            StudentId = "STU001",
            SchoolName = "Elementary School",
            GradeLevel = "3rd Grade",
            PrimaryDiagnosis = "ADHD",
            SecondaryDiagnoses = "Anxiety",
            Goals = "Improve focus and attention",
            Notes = "Responds well to positive reinforcement",
            IsActive = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        Assert.Equal(id, student.Id);
        Assert.Equal("John", student.FirstName);
        Assert.Equal("Doe", student.LastName);
        Assert.Equal(dateOfBirth, student.DateOfBirth);
        Assert.Equal("Male", student.Gender);
        Assert.Equal(therapistId, student.TherapistId);
        Assert.Equal("STU001", student.StudentId);
        Assert.Equal("Elementary School", student.SchoolName);
        Assert.Equal("3rd Grade", student.GradeLevel);
        Assert.Equal("ADHD", student.PrimaryDiagnosis);
        Assert.Equal("Anxiety", student.SecondaryDiagnoses);
        Assert.Equal("Improve focus and attention", student.Goals);
        Assert.Equal("Responds well to positive reinforcement", student.Notes);
        Assert.False(student.IsActive);
        Assert.Equal(createdAt, student.CreatedAt);
        Assert.Equal(updatedAt, student.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("John")]
    [InlineData("Mary Elizabeth")]
    [InlineData("José María")]
    public void Student_FirstName_ShouldAcceptValidValues(string firstName)
    {
        // Act
        var student = new Student { FirstName = firstName };

        // Assert
        Assert.Equal(firstName, student.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("Doe")]
    [InlineData("Van Der Berg")]
    [InlineData("O'Connor")]
    public void Student_LastName_ShouldAcceptValidValues(string lastName)
    {
        // Act
        var student = new Student { LastName = lastName };

        // Assert
        Assert.Equal(lastName, student.LastName);
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Other")]
    [InlineData("")]
    [InlineData(null)]
    public void Student_Gender_ShouldAcceptValidValues(string? gender)
    {
        // Act
        var student = new Student { Gender = gender };

        // Assert
        Assert.Equal(gender, student.Gender);
    }

    [Theory]
    [InlineData("K")]
    [InlineData("1st")]
    [InlineData("2nd Grade")]
    [InlineData("High School")]
    [InlineData("")]
    [InlineData(null)]
    public void Student_GradeLevel_ShouldAcceptValidValues(string? gradeLevel)
    {
        // Act
        var student = new Student { GradeLevel = gradeLevel };

        // Assert
        Assert.Equal(gradeLevel, student.GradeLevel);
    }

    [Fact]
    public void Student_IsActive_ShouldDefaultToTrue()
    {
        // Act
        var student = new Student();

        // Assert
        Assert.True(student.IsActive);
    }

    [Fact]
    public void Student_CreatedAt_ShouldBeSetToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var student = new Student();

        // Assert
        var afterCreation = DateTime.UtcNow;
        Assert.True(student.CreatedAt >= beforeCreation);
        Assert.True(student.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Student_UpdatedAt_ShouldBeSetToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var student = new Student();

        // Assert
        var afterCreation = DateTime.UtcNow;
        Assert.True(student.UpdatedAt >= beforeCreation);
        Assert.True(student.UpdatedAt <= afterCreation);
    }

    [Fact]
    public void Student_NavigationProperty_ShouldBeNullByDefault()
    {
        // Act
        var student = new Student();

        // Assert
        Assert.Null(student.Therapist);
    }

    [Fact]
    public void Student_NavigationProperty_ShouldBeSettable()
    {
        // Arrange
        var therapist = new User { FirstName = "Dr. Smith" };

        // Act
        var student = new Student { Therapist = therapist };

        // Assert
        Assert.Equal(therapist, student.Therapist);
    }

    [Fact]
    public void Student_WithLongStrings_ShouldAcceptWithinLimits()
    {
        // Arrange
        var longFirstName = new string('A', 100); // Max length for FirstName
        var longLastName = new string('B', 100);  // Max length for LastName
        var longStudentId = new string('C', 100); // Max length for StudentId
        var longSchoolName = new string('D', 200); // Max length for SchoolName
        var longGradeLevel = new string('E', 50);  // Max length for GradeLevel
        var longPrimaryDiagnosis = new string('F', 200); // Max length for PrimaryDiagnosis
        var longSecondaryDiagnoses = new string('G', 500); // Max length for SecondaryDiagnoses
        var longGoals = new string('H', 1000); // Max length for Goals
        var longNotes = new string('I', 1000); // Max length for Notes

        // Act
        var student = new Student
        {
            FirstName = longFirstName,
            LastName = longLastName,
            StudentId = longStudentId,
            SchoolName = longSchoolName,
            GradeLevel = longGradeLevel,
            PrimaryDiagnosis = longPrimaryDiagnosis,
            SecondaryDiagnoses = longSecondaryDiagnoses,
            Goals = longGoals,
            Notes = longNotes
        };

        // Assert
        Assert.Equal(longFirstName, student.FirstName);
        Assert.Equal(longLastName, student.LastName);
        Assert.Equal(longStudentId, student.StudentId);
        Assert.Equal(longSchoolName, student.SchoolName);
        Assert.Equal(longGradeLevel, student.GradeLevel);
        Assert.Equal(longPrimaryDiagnosis, student.PrimaryDiagnosis);
        Assert.Equal(longSecondaryDiagnoses, student.SecondaryDiagnoses);
        Assert.Equal(longGoals, student.Goals);
        Assert.Equal(longNotes, student.Notes);
    }

    [Fact]
    public void Student_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var specialChars = "Test with special chars: <>\"'&@#$%^*()[]{}";

        // Act
        var student = new Student
        {
            FirstName = specialChars,
            LastName = specialChars,
            StudentId = specialChars,
            SchoolName = specialChars,
            PrimaryDiagnosis = specialChars,
            SecondaryDiagnoses = specialChars,
            Goals = specialChars,
            Notes = specialChars
        };

        // Assert
        Assert.Equal(specialChars, student.FirstName);
        Assert.Equal(specialChars, student.LastName);
        Assert.Equal(specialChars, student.StudentId);
        Assert.Equal(specialChars, student.SchoolName);
        Assert.Equal(specialChars, student.PrimaryDiagnosis);
        Assert.Equal(specialChars, student.SecondaryDiagnoses);
        Assert.Equal(specialChars, student.Goals);
        Assert.Equal(specialChars, student.Notes);
    }

    [Theory]
    [InlineData(2000, 1, 1)]
    [InlineData(2010, 6, 15)]
    [InlineData(2020, 12, 31)]
    public void Student_DateOfBirth_ShouldAcceptValidDates(int year, int month, int day)
    {
        // Arrange
        var dateOfBirth = new DateTime(year, month, day);

        // Act
        var student = new Student { DateOfBirth = dateOfBirth };

        // Assert
        Assert.Equal(dateOfBirth, student.DateOfBirth);
    }

    [Fact]
    public void Student_DateOfBirth_ShouldAcceptNull()
    {
        // Act
        var student = new Student { DateOfBirth = null };

        // Assert
        Assert.Null(student.DateOfBirth);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Student_IsActive_ShouldAcceptBothValues(bool isActive)
    {
        // Act
        var student = new Student { IsActive = isActive };

        // Assert
        Assert.Equal(isActive, student.IsActive);
    }

    [Fact]
    public void Student_TherapistId_ShouldAcceptGuidValues()
    {
        // Arrange
        var therapistId1 = Guid.NewGuid();
        var therapistId2 = Guid.Empty;

        // Act
        var student1 = new Student { TherapistId = therapistId1 };
        var student2 = new Student { TherapistId = therapistId2 };

        // Assert
        Assert.Equal(therapistId1, student1.TherapistId);
        Assert.Equal(therapistId2, student2.TherapistId);
    }

    [Fact]
    public void Student_Timestamps_ShouldBeIndependentlySettable()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var student = new Student
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        Assert.Equal(createdAt, student.CreatedAt);
        Assert.Equal(updatedAt, student.UpdatedAt);
    }

    [Fact]
    public void Student_AllOptionalFields_ShouldAcceptNull()
    {
        // Act
        var student = new Student
        {
            DateOfBirth = null,
            Gender = null,
            StudentId = null,
            SchoolName = null,
            GradeLevel = null,
            PrimaryDiagnosis = null,
            SecondaryDiagnoses = null,
            Goals = null,
            Notes = null,
            Therapist = null
        };

        // Assert
        Assert.Null(student.DateOfBirth);
        Assert.Null(student.Gender);
        Assert.Null(student.StudentId);
        Assert.Null(student.SchoolName);
        Assert.Null(student.GradeLevel);
        Assert.Null(student.PrimaryDiagnosis);
        Assert.Null(student.SecondaryDiagnoses);
        Assert.Null(student.Goals);
        Assert.Null(student.Notes);
        Assert.Null(student.Therapist);
    }

    [Fact]
    public void Student_RequiredFields_ShouldNotBeNull()
    {
        // Arrange & Act
        var student = new Student
        {
            FirstName = "Required",
            LastName = "Field",
            TherapistId = Guid.NewGuid()
        };

        // Assert
        Assert.NotNull(student.FirstName);
        Assert.NotNull(student.LastName);
        Assert.NotEqual(Guid.Empty, student.TherapistId);
    }
}