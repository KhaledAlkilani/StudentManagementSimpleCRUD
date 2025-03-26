using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Adjust these namespaces to match your project structure:
using StudentManagement.Data;
using StudentManagement.Models;
using StudentManagement.Controllers;

namespace StudentManagementTests
{
    public class StudentsControllerTests
    {
        // -------------------------
        // POST Tests
        // -------------------------
        [Fact]
        public async Task PostStudent_ReturnsCreatedAtActionResult()
        {
            // Arrange: create in-memory database options
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Post")
                .Options;

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var newStudent = new Student { Id = 10, FirstName = "John", LastName = "Doe", Age = 30 };

                // Act: call PostStudent
                var actionResult = await controller.PostStudent(newStudent);

                // Assert: ensure result is CreatedAtActionResult and the student is added
                var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var returnedStudent = Assert.IsAssignableFrom<Student>(createdResult.Value);
                Assert.Equal(newStudent.Id, returnedStudent.Id);
                Assert.Equal("John", returnedStudent.FirstName);
            }
        }

        // -------------------------
        // PUT Tests
        // -------------------------
        [Fact]
        public async Task PutStudent_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange: use unique in-memory database
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Put")
                .Options;

            // Seed the database with a student
            using (var context = new StudentContext(options))
            {
                var student = new Student { Id = 1, FirstName = "Alice", LastName = "Smith", Age = 25 };
                context.Students.Add(student);
                context.SaveChanges();
            }

            // Act & Assert: update the student's info
            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var updatedStudent = new Student { Id = 1, FirstName = "Alice", LastName = "Johnson", Age = 26 };

                var result = await controller.PutStudent(1, updatedStudent);
                Assert.IsType<NoContentResult>(result);

                // Verify the changes were saved
                var studentInDb = await context.Students.FindAsync(1);
                Assert.Equal("Johnson", studentInDb.LastName);
                Assert.Equal(26, studentInDb.Age);
            }
        }

        [Fact]
        public async Task PutStudent_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange: use in-memory database with no seeding needed for this test
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_PutMismatch")
                .Options;

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var updatedStudent = new Student { Id = 2, FirstName = "Alice", LastName = "Smith", Age = 25 };

                // Act: try updating with a mismatched id
                var result = await controller.PutStudent(1, updatedStudent);

                // Assert: expect a BadRequestResult due to id mismatch
                Assert.IsType<BadRequestResult>(result);
            }
        }

        // -------------------------
        // DELETE Tests
        // -------------------------
        [Fact]
        public async Task DeleteStudent_ReturnsNoContent_WhenStudentIsDeleted()
        {
            // Arrange: create in-memory database and seed a student
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Delete")
                .Options;

            using (var context = new StudentContext(options))
            {
                var student = new Student { Id = 1, FirstName = "Bob", LastName = "Brown", Age = 28 };
                context.Students.Add(student);
                context.SaveChanges();
            }

            // Act & Assert: delete the student and check the response
            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var result = await controller.DeleteStudent(1);
                Assert.IsType<NoContentResult>(result);

                // Verify the student is removed from the database
                var deletedStudent = await context.Students.FindAsync(1);
                Assert.Null(deletedStudent);
            }
        }

        [Fact]
        public async Task DeleteStudent_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            // Arrange: set up a new in-memory database with no data
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_DeleteNotFound")
                .Options;

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);

                // Act: attempt to delete a non-existent student
                var result = await controller.DeleteStudent(999);

                // Assert: the result should be NotFound
                Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}
