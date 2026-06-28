using System;
using System.Collections.Generic;
using Npgsql;

namespace scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== RE-SYNCING CLASS_ID FROM CLASS_DB TO USER_DB ===");

            string classConnStr =
                "Host=localhost;Port=5432;Database=ClassDb;Username=postgres;Password=123456";
            string userConnStr =
                "Host=localhost;Port=5432;Database=user_db;Username=postgres;Password=123456";

            var currentAssignments = new List<(Guid StudentId, Guid ClassId)>();
            var graduatedStudents = new List<Guid>();

            // 1. Fetch current class assignments from ClassDb
            try
            {
                using var conn = new NpgsqlConnection(classConnStr);
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "SELECT \"StudentId\", \"ClassId\", \"PromotionStatus\", \"IsCurrent\" FROM \"StudentClasses\"",
                    conn
                );
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var studentId = reader.GetGuid(0);
                    var classId = reader.GetGuid(1);
                    var promoStatus = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var isCurrent = reader.GetBoolean(3);

                    if (isCurrent)
                    {
                        currentAssignments.Add((studentId, classId));
                    }
                    else if (promoStatus == "Graduated")
                    {
                        graduatedStudents.Add(studentId);
                    }
                }
                Console.WriteLine(
                    $"Found {currentAssignments.Count} current assignments and {graduatedStudents.Count} graduated students in ClassDb."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR reading ClassDb: {ex.Message}");
                return;
            }

            // 2. Update user_db
            try
            {
                using var conn = new NpgsqlConnection(userConnStr);
                conn.Open();

                foreach (var item in currentAssignments)
                {
                    using var cmd = new NpgsqlCommand(
                        "UPDATE \"Users\" SET \"ClassId\" = @classId, \"StudentStatus\" = 0 WHERE \"Id\" = @studentId",
                        conn
                    );
                    cmd.Parameters.AddWithValue("classId", item.ClassId);
                    cmd.Parameters.AddWithValue("studentId", item.StudentId);
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine(
                            $"Synced Student {item.StudentId} to Class {item.ClassId} (Active)."
                        );
                    }
                }

                foreach (var studentId in graduatedStudents)
                {
                    using var cmd = new NpgsqlCommand(
                        "UPDATE \"Users\" SET \"ClassId\" = NULL, \"StudentStatus\" = 1 WHERE \"Id\" = @studentId",
                        conn
                    );
                    cmd.Parameters.AddWithValue("studentId", studentId);
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Console.WriteLine($"Synced Student {studentId} to Graduated.");
                    }
                }

                Console.WriteLine("Re-sync completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR updating user_db: {ex.Message}");
            }
        }
    }
}
