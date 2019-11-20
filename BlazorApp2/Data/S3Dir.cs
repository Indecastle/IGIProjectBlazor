using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Data
{
    public enum S3TypeFile
    {
        Files, Settings, Account
    }
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector)
        {
            return enumerable.GroupBy(keySelector).Select(grp => grp.First());
        }
    }



    public class S3Dir
    {
        public readonly string rootPath;
        public readonly int rootLevel = 3;
        public IS3Service _is3 { get; set; }
        public bool IsUpdating = false;
        public string UserName { get; private set; }
        public int Level { get; private set; }
        public S3DirObject CurrentDir { get; private set; }
        public S3DirObject OldDir { get; private set; }
        //public string DirPath { get; private set; }
        //public string DirName { get; private set; }
        public string BackDirPath { get; private set; }
        public string BackDirName { get; private set; }
        public bool IsRoot { get; private set; }
        public List<S3DirObject> SubDirs { get; private set; } = new List<S3DirObject>();
        public List<S3FileObject> S3Objs { get; private set; } = new List<S3FileObject>();
        public Stack<S3DirObject> DirStack { get; private set; } = new Stack<S3DirObject>();
        public S3Dir()
        {
            S3Objs = new List<S3FileObject>();
        }
        public S3Dir(IS3Service service, string userName, S3TypeFile typeFile)
        {
            rootLevel = 3;
            _is3 = service;
            UserName = userName;
            rootPath = $"Users/{userName}/{typeFile.ToString()}";
            //BackDirName = BackDirPath = "";
            //DirPath = DirName = userName;
            CurrentDir = new S3DirObject
            {
                Name = typeFile.ToString(),
                FullPathName = rootPath
            };
            IsRoot = true;
            Level = rootLevel;
        }
        public S3Dir(IS3Service service, string rootPath)
        {
            rootLevel = rootPath == "" ? 0 : rootPath.Split('/').Length;
            _is3 = service;
            UserName = "";
            this.rootPath = rootPath;
            //Console.WriteLine("----------------- " + rootPath);
            //BackDirName = BackDirPath = "";
            //DirPath = DirName = userName;
            CurrentDir = new S3DirObject
            {
                Name = "Root",
                FullPathName = rootPath
            };
            IsRoot = true;
            Level = rootLevel;
        }

        public async Task UpdateDirAsync()
        {
            SubDirs.Clear();
            S3Objs.Clear();

            var S3ListFiles = await _is3.ListFilesAsync(CurrentDir.FullPathName);
            IEnumerable<S3Object> diststrs = S3ListFiles.Where((s) =>
            {
                return s.Key.StartsWith(CurrentDir.FullPathName);
            }).DistinctBy((s) =>
            {
                string[] ar2 = s.Key.Split('/');
                return ar2[Level];
            });

            foreach (var s in diststrs)
            {
                string[] ar2 = s.Key.Split('/');
                if (ar2[Level] != "")
                    if (ar2.Length - 1 == Level)
                        S3Objs.Add(new S3FileObject
                        {
                            Name = ar2[Level],
                            DirPath = CurrentDir.FullPathName,
                            FullPathName = s.Key,
                            Owner = s.Owner,
                            Size = s.Size,
                            LastModified = s.LastModified,
                            ETag = s.ETag,
                            StorageClass = s.StorageClass.Value
                        });
                    else
                        SubDirs.Add(new S3DirObject
                        {
                            FullPathName = String.Join("/", ar2.Take(Level + 1)),
                            Name = ar2[Level]
                        });
            }

            //foreach (var dir in SubDirs)
            //{
            //    Console.WriteLine("========  Name = {0}    |    FullName = {1}",
            //        dir.Name, dir.FullPathName);
            //}
        }

        public async Task SubDirAsync(S3DirObject subobj)
        {
            Level++;
            IsRoot = (Level == rootLevel);
            OldDir = CurrentDir;
            DirStack.Push(CurrentDir);
            CurrentDir = subobj;

            await UpdateDirAsync();
        }

        public async Task BackDirAsync(int level)
        {
            if (!IsRoot)
            {
                for(int i = 0; i < level; i++)
                {
                    Level--;
                    CurrentDir = DirStack.Pop();
                }
                await UpdateDirAsync();
            }
            if (Level == rootLevel) // IsRoot
            {
                IsRoot = true;
                //BackDirName = BackDirPath = "";
            }
            else
            {
                OldDir = DirStack.Peek();
            }

        }

        public async Task UploadFileAsync(MemoryStream ms, string fileName, bool upload = false)
        {
            await _is3.UploadObjectAsync(ms, CurrentDir.FullPathName + '/' + fileName);
            if (upload)
                await UpdateDirAsync();
        }
        public async Task CreateFolderAsync(string newDirName, bool upload = false)
        {
            await _is3.CreateFolderAsync(CurrentDir.FullPathName + '/' + newDirName + '/');
            if (upload)
                await UpdateDirAsync();
        }
    }






    public class S3FileObject : IS3Object
    {
        public string Name { get; set; }
        public string DirPath { get; set; }
        public string FullPathName { get; set; }
        public long Size { get; set; }
        public Owner Owner { get; set; }
        public DateTime LastModified { get; set; }
        public string ETag { get; set; }
        public string StorageClass { get; set; }
    }

    public class S3DirObject : IS3Object
    {
        public string Name { get; set; }
        public string FullPathName { get; set; }
    }

    public interface IS3Object
    {
        string FullPathName { get; set; }
        string Name { get; set; }
    }
}
