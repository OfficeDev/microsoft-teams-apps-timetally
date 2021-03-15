// <copyright file="RepositoryAccessors.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System.Threading.Tasks;

    /// <summary>
    /// This class lists all repository instances which will be used to perform database operations on entities.
    /// </summary>
    public class RepositoryAccessors : IRepositoryAccessors
    {
        /// <summary>
        /// Holds the instance of timesheet context.
        /// </summary>
        private readonly TimesheetContext context;

        /// <summary>
        /// Holds the instance of project repository which manages storage operations related to projects.
        /// </summary>
        private IProjectRepository projectRepository;

        /// <summary>
        /// Holds the instance of conversation repository which manages storage operations related to bot-user conversation.
        /// </summary>
        private IConversationRepository conversationRepository;

        /// <summary>
        /// Holds the instance of timesheet repository which manages storage operations related to timesheets.
        /// </summary>
        private ITimesheetRepository timesheetRepository;

        /// <summary>
        /// Holds the instance of task repository which manages storage operations related to task.
        /// </summary>
        private ITaskRepository taskRepository;

        /// <summary>
        /// Holds the instance of member repository which manages storage operations related to project members.
        /// </summary>
        private IMemberRepository memberRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryAccessors"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public RepositoryAccessors(TimesheetContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets instance of project repository to manage database operations on project entity.
        /// </summary>
        public IProjectRepository ProjectRepository
        {
            get
            {
                if (this.projectRepository == null)
                {
                    this.projectRepository = new ProjectRepository(this.context);
                }

                return this.projectRepository;
            }
        }

        /// <summary>
        /// Gets instance of timesheet repository to manage database operations on timesheet entity.
        /// </summary>
        public ITimesheetRepository TimesheetRepository
        {
            get
            {
                if (this.timesheetRepository == null)
                {
                    this.timesheetRepository = new TimesheetRepository(this.context);
                }

                return this.timesheetRepository;
            }
        }

        /// <summary>
        /// Gets instance of task repository to manage database operations on task entity.
        /// </summary>
        public ITaskRepository TaskRepository
        {
            get
            {
                if (this.taskRepository == null)
                {
                    this.taskRepository = new TaskRepository(this.context);
                }

                return this.taskRepository;
            }
        }

        /// <summary>
        /// Gets instance of member mapping repository to manage database operations on user project mapping entity.
        /// </summary>
        public IMemberRepository MemberRepository
        {
            get
            {
                if (this.memberRepository == null)
                {
                    this.memberRepository = new MemberRepository(this.context);
                }

                return this.memberRepository;
            }
        }

        /// <summary>
        /// Gets instance of project repository to manage database operations on user conversation entity.
        /// </summary>
        public IConversationRepository ConversationRepository
        {
            get
            {
                if (this.conversationRepository == null)
                {
                    this.conversationRepository = new ConversationRepository(this.context);
                }

                return this.conversationRepository;
            }
        }

        /// <summary>
        /// Gets the timesheet context.
        /// </summary>
        public TimesheetContext Context
        {
            get
            {
                return this.context;
            }
        }

        /// <summary>
        /// Saves changes made till now to database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await this.context.SaveChangesAsync();
        }
    }
}