using System;
using System.Collections.Generic;
using System.IO;
using Project2015To2017.Definition;
using Project2015To2017.Writing;
using Serilog;

namespace Project2015To2017.Migrate2017.Tool
{
	public partial class CommandLogic
	{
		private void WizardModernCleanUp(IReadOnlyList<Project> modern, ITransformationSet transformationSet,
			ConversionOptions conversionOptions)
		{
			var transformations = transformationSet.CollectAndOrderTransformations(facility.Logger, conversionOptions);

			var writer = new ProjectWriter(facility.Logger, new ProjectWriteOptions());

			foreach (var project in modern)
			{
				using (facility.Logger.BeginScope(project.FilePath))
				{
					var projectName = Path.GetFileNameWithoutExtension(project.FilePath.Name);
					Log.Information("Processing {ProjectName}...", projectName);

					if (!project.Valid)
					{
						Log.Error("Project {ProjectName} is marked as invalid, skipping...", projectName);
						continue;
					}

					foreach (var transformation in transformations.WhereSuitable(project, conversionOptions))
					{
						try
						{
							transformation.Transform(project);
						}
						catch (Exception e)
						{
							Log.Error(e, "Transformation {Item} has thrown an exception, skipping...",
								transformation.GetType().Name);
						}
					}

					if (!writer.TryWrite(project))
						continue;
					Log.Information("Project {ProjectName} has been processed", projectName);
				}
			}
		}
	}
}