using XForensics.Core.FileSystem;
using System.Collections.Generic;
using System.Linq;

namespace XForensics.Core.Database
{
    public class FileDatabase
    {
        /// <summary>
        /// Map of files according to its offset
        /// </summary>
        Dictionary<long, DatabaseFile> files;

        /// <summary>
        /// List of files at the root of the file system.
        /// </summary>
        List<DatabaseFile> root;

        /// <summary>
        /// Volume associated with this database.
        /// </summary>
        Volume volume;

        public FileDatabase(Volume volume)
        {
            this.files = new Dictionary<long, DatabaseFile>();
            this.root = new List<DatabaseFile>();
            this.volume = volume;

            MergeActiveFileSystem(volume);
        }

        /// <summary>
        /// Returns the number of files in this database.
        /// </summary>
        /// <returns>Number of files in this database.</returns>
        public int Count()
        {
            return files.Count;
        }

        /// <summary>
        /// Update the file system in this database. This should be called after 
        /// any modifications are made to the database.
        /// </summary>
        public void Update()
        {
            // TODO: Only update affected files

            // Construct a new file system.
            root = new List<DatabaseFile>();

            // Link the file system together.
            LinkFileSystem();
        }

        public void Reset()
        {
            this.files = new Dictionary<long, DatabaseFile>();

            MergeActiveFileSystem(this.volume);
        }

        /// <summary>
        /// Build the file system.
        /// </summary>
        private void LinkFileSystem()
        {
            // Clear all previous links.
            foreach (var file in files.Values)
            {
                file.Children = new List<DatabaseFile>();
                file.SetParent(null);
            }

            // Build an index from directory clusters to directories. The old implementation
            // compared every directory with every file (O(nÂ²)); metadata recovery can easily
            // produce tens of thousands of entries, so indexing keeps relinking near O(n).
            var directoryByCluster = new Dictionary<uint, List<DatabaseFile>>();
            foreach (var directory in files.Values.Where(f => f.IsDirectory()))
            {
                foreach (var cluster in directory.ClusterChain)
                {
                    if (!directoryByCluster.TryGetValue(cluster, out var parents))
                    {
                        parents = new List<DatabaseFile>();
                        directoryByCluster[cluster] = parents;
                    }
                    if (!parents.Contains(directory))
                        parents.Add(directory);
                }
            }

            foreach (var file in files.Values)
            {
                if (!directoryByCluster.TryGetValue(file.Cluster, out var parents))
                    continue;

                foreach (var parent in parents)
                {
                    if (!parent.Children.Contains(file))
                        parent.Children.Add(file);
                    if (file.GetParent() != parent)
                        file.SetParent(parent);
                }
            }

            // Gather files at the root.
            root.AddRange(files.Values.Where(file => !file.HasParent()));
        }

        /// <summary>
        /// Merge the active file system into this database.
        /// </summary>
        /// <param name="volume">The active file system.</param>
        private void MergeActiveFileSystem(Volume volume)
        {
            RegisterDirectoryEntries(volume.GetRoot());
        }

        /// <summary>
        /// Register directory entries in bulk.
        /// </summary>
        /// <param name="dirents"></param>
        private void RegisterDirectoryEntries(List<DirectoryEntry> dirents)
        {
            foreach (var dirent in dirents)
            {
                if (dirent.IsDeleted())
                {
                    AddFile(dirent, true);
                }
                else
                {
                    AddFile(dirent, false);

                    if (dirent.IsDirectory())
                    {
                        RegisterDirectoryEntries(dirent.Children);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a new cluster chain for a deleted file.
        /// </summary>
        /// <param name="dirent">The deleted file.</param>
        /// <returns></returns>
        private List<uint> GenerateArtificialClusterChain(DirectoryEntry dirent)
        {
            // TODO: Check for zeroed FirstCluster

            if (dirent.IsDirectory())
            {
                // NOTE: Directories with more than one 256 files would have multiple clusters
                return new List<uint>() { dirent.FirstCluster };
            }
            else
            {
                var clusterCount = (int)(((dirent.FileSize + (this.volume.BytesPerCluster - 1)) &
                         ~(this.volume.BytesPerCluster - 1)) / this.volume.BytesPerCluster);

                return Enumerable.Range((int)dirent.FirstCluster, clusterCount).Select(i => (uint)i).ToList();
            }
        }

        /// <summary>
        /// Get a file by the offset into the file system.
        /// </summary>
        /// <param name="offset">File area offset</param>
        /// <returns>DatabaseFile from this database</returns>
        public DatabaseFile GetFile(long offset)
        {
            if (files.ContainsKey(offset))
            {
                return files[offset];
            }

            return null;
        }

        /// <summary>
        /// Get a file by an instance of a DirectoryEntry.
        /// </summary>
        /// <param name="dirent">DirectoryEntry instance</param>
        /// <returns>DatabaseFile from this database</returns>
        public DatabaseFile GetFile(DirectoryEntry dirent)
        {
            if (files.ContainsKey(dirent.Offset))
            {
                return files[dirent.Offset];
            }

            return null;
        }

        /// <summary>
        /// Create and initialize a new DatabaseFile for this DirectoryEntry.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="dirent"></param>
        /// <param name="deleted"></param>
        private DatabaseFile CreateDatabaseFile(DirectoryEntry dirent, bool deleted)
        {
            files[dirent.Offset] = new DatabaseFile(dirent, deleted);
            if (deleted)
            {
                files[dirent.Offset].ClusterChain = GenerateArtificialClusterChain(dirent);
            }
            else
            {
                files[dirent.Offset].ClusterChain = this.volume.GetClusterChain(dirent);
            }

            return files[dirent.Offset];
        }

        /// <summary>
        /// Add and initialize a new DatabaseFile.
        /// </summary>
        /// <param name="dirent">The associated DirectoryEntry</param>
        /// <param name="deleted">Whether or not this file was deleted</param>
        public DatabaseFile AddFile(DirectoryEntry dirent, bool deleted)
        {
            // Create the file if it was not already added
            if (!files.ContainsKey(dirent.Offset))
            {
                return CreateDatabaseFile(dirent, deleted);
            }

            return files[dirent.Offset];
        }

        /// <summary>
        /// Get all files from this database.
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, DatabaseFile> GetFiles()
        {
            return files;
        }

        /// <summary>
        /// Get the root files from this database's file system.
        /// </summary>
        /// <returns></returns>
        public List<DatabaseFile> GetRootFiles()
        {
            return root;
        }

        /// <summary>
        /// Get the volume associated with this database.
        /// </summary>
        /// <returns></returns>
        public Volume GetVolume()
        {
            return volume;
        }
    }
}
