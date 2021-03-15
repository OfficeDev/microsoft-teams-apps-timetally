// <copyright file="TasksValidationAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.GoodReads.Helpers.CustomValidations
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Validate tasks based on length and task count for project.
    /// </summary>
    public sealed class TasksValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TasksValidationAttribute"/> class.
        /// </summary>
        /// <param name="maxCount">Max count of tasks per project.</param>
        /// <param name="titleMaxLength">Max length of task title.</param>
        public TasksValidationAttribute(int maxCount, int titleMaxLength = 20)
        {
            this.MaxCount = maxCount;
            this.TitleMaxLength = titleMaxLength;
        }

        /// <summary>
        /// Gets max count of tasks for validation.
        /// </summary>
        public int MaxCount { get; }

        /// <summary>
        /// Gets max task title length for validation.
        /// </summary>
        public int TitleMaxLength { get; }

        /// <summary>
        /// Validate tasks based on title length and number of tasks per project.
        /// </summary>
        /// <param name="value">List containing tasks.</param>
        /// <param name="validationContext">Context for getting object which needs to be validated.</param>
        /// <returns>Validation result (either error message for failed validation or success).</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Task list is required.");
            }

            if (value.GetType() != typeof(List<TaskDTO>))
            {
                return new ValidationResult("Expected type for parameter is list.");
            }

            var tasks = (List<TaskDTO>)value;
            if (tasks.Count > this.MaxCount)
            {
                return new ValidationResult("Total number of tasks has exceeded max count of {this.MaxCount}");
            }

            foreach (var task in tasks)
            {
                if (task == null)
                {
                    return new ValidationResult("Task cannot be null");
                }

                if (task.Title.Length > this.TitleMaxLength)
                {
                    return new ValidationResult("Task title length has exceeded max count of {this.TitleMaxLength}");
                }
            }

            // Tasks are valid.
            return ValidationResult.Success;
        }
    }
}