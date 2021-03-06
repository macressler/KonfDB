﻿#region License and Product Information

// 
//     This file 'GetEnv.cs' is part of KonfDB application - 
//     a project perceived and developed by Punit Ganshani.
// 
//     KonfDB is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     KonfDB is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with KonfDB.  If not, see <http://www.gnu.org/licenses/>.
// 
//     You can also view the documentation and progress of this project 'KonfDB'
//     on the project website, <http://www.konfdb.com> or on 
//     <http://www.ganshani.com/applications/konfdb>

#endregion

using KonfDB.Infrastructure.Common;
using KonfDB.Infrastructure.Database.Entities.Configuration;
using KonfDB.Infrastructure.Database.Enums;
using KonfDB.Infrastructure.Services;
using KonfDB.Infrastructure.Shell;

namespace KonfDB.Engine.Commands.Shared
{
    internal class GetEnv : IAuthCommand
    {
        private const string DoesNotExist = "Environment does not exist";

        public string Keyword
        {
            get { return "GetEnv"; }
        }

        public string Command
        {
            get { return "GetEnv { /name:EnvironmentName | /id:EnvironmentId }"; }
        }

        public string Help
        {
            get { return "Get Environment details"; }
        }

        public bool IsValid(CommandInput input)
        {
            return input.HasArgument("name") || input.HasArgument("id");
        }

        public CommandOutput OnExecute(CommandInput arguments)
        {
            var output = new CommandOutput {PostAction = CommandOutput.PostCommandAction.None};
            bool completed = false;
            EnvironmentModel model = default(EnvironmentModel);
            long userId = arguments.GetUserId();

            if (arguments["name"] != null)
            {
                model = CurrentHostContext.Default.Provider.ConfigurationStore.GetEnvironment(userId, arguments["name"]);
                completed = true;
            }
            else if (arguments["id"] != null)
            {
                long envId = -1;
                long.TryParse(arguments["id"], out envId);

                if (envId != -1)
                {
                    model = CurrentHostContext.Default.Provider.ConfigurationStore.GetEnvironment(userId, envId);
                    completed = true;
                }
            }

            if (!completed)
            {
                output.DisplayMessage = "Invalid data in command received";
                output.PostAction = CommandOutput.PostCommandAction.ShowCommandHelp;
                output.MessageType = CommandOutput.DisplayMessageType.Error;
            }
            else if (model != null)
            {
                output.Data = model;
                output.DisplayMessage = "Success";
            }
            else
            {
                output.DisplayMessage = DoesNotExist;
                output.MessageType = CommandOutput.DisplayMessageType.Error;
            }

            return output;
        }

        public AppType Type
        {
            get { return AppType.Client | AppType.Server; }
        }

        public AuditRecordModel GetAuditCommand(CommandInput input)
        {
            return new AuditRecordModel
            {
                Area = AuditArea.Environment,
                Reason = AuditReason.Retrieved,
                Message = input.Command,
                Key = input.HasArgument("name") ? input["name"] : input["id"],
                UserId = input.GetUserId()
            };
        }
    }
}