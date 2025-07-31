using System.Linq.Expressions;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<IEnumerable<Student>> GetByTherapistAsync(Guid therapistId, bool includeInactive = false);
    Task<IEnumerable<Student>> SearchStudentsAsync(
        Guid therapistId,
        string? searchTerm = null,
        int? gradeLevel = null,
        bool? isActive = null,
        int skip = 0,
        int take = 20);
    Task<string> GenerateUniqueAccessCodeAsync();
    Task<IEnumerable<StudentGoal>> GetStudentGoalsAsync(
        Guid? studentId = null,
        Guid? therapistId = null,
        GoalStatus? status = null,
        string? goalArea = null,
        int skip = 0,
        int take = 20);
    Task<StudentGoal> CreateGoalAsync(StudentGoal goal);
    Task UpdateGoalAsync(StudentGoal goal);
    Task DeleteGoalAsync(Guid goalId);
}