using CSS.Cloud.Framework;
using CSS.Connector.FileProcessing.Core.Models;
 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSS.Connector.FileProcessing.Core
{
    /// <summary>
    /// Entity Framework context class for Files.
    /// </summary>
    public class FileContext : DbContext
    {
		public FileContext()
		{
		}

		public FileContext(DbContextOptions<FileContext> options)
		: base(options)
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
			// Use next line only when generating DB schema or running tests; Then comment out & uncomment following two
			// optionsBuilder.UseSqlServer("Persist Security Info = False; Integrated Security = True; Initial Catalog = Connector; Server = cloud-sql2017;", providerOptions => providerOptions.CommandTimeout(60));
			string connectionString = ApplicationSettingsManager.GetConfigParameter("Config", "DataSources", "ConnectorConnectionString");
			optionsBuilder.UseSqlServer(connectionString, providerOptions => providerOptions.CommandTimeout(60));
		}

		/// <summary>
		/// Returns list of FileDefinitions
		/// </summary>
		/// <returns></returns>
		internal List<FileDefinition> GetFiles()
        {
			return FileDefinitions.ToList<FileDefinition>();
        }

        /// <summary>
        /// Called by Entity Framework.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileDefinition>()
                .HasKey(c => new { c.Id });

            modelBuilder.Entity<FileInstance>()
                .HasKey(c => new { c.InstanceId });

            modelBuilder.Entity<FileWatcherFolder>()
               .HasKey(c => new { c.WatchingPath });

            modelBuilder.Entity<FileEventLog>()
               .HasKey(c => new { c.Id });

			modelBuilder.Entity<FileProcessorConfig>()
				.HasKey(c => new { c.Processor });

			modelBuilder.Entity<TypeMapping>()
				.HasKey(c => new { c.FileId, c.KeyType });
        }

        /// <summary>
        /// Entity Framework DbSet for FileDefinition
        /// </summary>
        public DbSet<FileDefinition> FileDefinitions { get; set; }

        /// <summary>
        /// Entity Framework DbSet for FileWatcherFolder
        /// </summary>
        public DbSet<FileWatcherFolder> FileWatcherFolders { get; set; }

        /// <summary>
        /// Entity Framework DbSet for FileEventLog
        /// </summary>
        public DbSet<FileEventLog> FileEventLogs { get; set; }

        /// <summary>
        /// Entity Framework DbSet for FileInstance
        /// </summary>
        public DbSet<FileInstance> FileInstances { get; set; }

		/// <summary>
		/// Entity Framework DbSet for FileProcessorConfigs
		/// </summary>
		public DbSet<FileProcessorConfig> FileProcessorConfigs { get; set; }

		/// <summary>
		/// Entity Framework DbSet for TypeMappings
		/// </summary>
		public DbSet<TypeMapping> TypeMappings{ get; set; }

		public List<FileWatcherFolder> GetFileWatcherFolders()
        {
            return FileWatcherFolders.ToList();
        }

        /// <summary>
        /// Saves a FileInstance
        /// </summary>
        /// <param name="fileInstance"></param>
        /// <returns></returns>
        internal FileInstance SaveFileInstance(FileInstance fileInstance)
        {
            if (fileInstance.InstanceId != null && fileInstance.InstanceId.Length > 0 && !fileInstance.InstanceId.StartsWith("new:"))
            {
                FileInstances.Update(fileInstance);
                this.SaveChanges();
            }
            else
            {
				if (fileInstance.InstanceId == null || fileInstance.InstanceId.Length == 0) fileInstance.InstanceId = Guid.NewGuid().ToString();
				if (fileInstance.InstanceId.StartsWith("new:")) fileInstance.InstanceId = fileInstance.InstanceId.Remove(0, 4);
				FileInstances.Add(fileInstance);
                this.SaveChanges();
            }
            return fileInstance;
        }

        /// <summary>
        /// Write a new FileEventLog
        /// </summary>
        /// <param name="log"></param>
        public void WriteFileEventLog(CSS.Connector.FileProcessing.Core.Models.FileEventLog log)
        {
            FileEventLogs.Add(log);
            this.SaveChanges();
        }

        /// <summary>
        /// Checks for a duplicate run of a file.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        internal bool CheckForDuplicate(int id, string hashCode)
        {
            return FileInstances.Where(i => i.FileId == id && i.HashCode == hashCode).Any();
        }

        internal List<FileInstance> GetFileInstances(DateTime startDate, DateTime endDate)
        {
            return FileInstances.Where(i => i.BeginTime > startDate && i.BeginTime <= endDate).OrderBy(i => i.BeginTime).ToList();
        }

		internal string GetFileProcessorConfig(string processor)
		{
			var configs = FileProcessorConfigs.Where(i => i.Processor == processor).ToList();
			if (configs == null || configs.Count != 1) throw new System.Data.DataException(string.Format("There were {0} configs returned for processor {1} in table FileProcessorConfigs.", (configs == null ? "no" : configs.Count.ToString()), processor));
			return configs[0].Config;
		}

		internal string GetTypeMapping(string fileId, string keyType)
		{
			var mappings = TypeMappings.Where(i => i.FileId == fileId && i.KeyType == keyType).ToList();
			if (mappings == null || mappings.Count > 1) throw new System.Data.DataException(string.Format("One TypeMapping row expected for FileId ({0}), KeyType ({1}).  ({2}) were found.", fileId, keyType, mappings.Count.ToString()));
			if (mappings.Count == 0) return null; // Let caller decide what to do if not found
			return mappings[0].Value;
		}
	}
}
