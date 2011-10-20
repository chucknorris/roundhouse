using System;
using System.Collections.Generic;
using Bottles.Diagnostics;
using System.Linq;
using FubuCore;

namespace Bottles.Deployment.Runtime.Content
{
    public interface IBottleMover
    {
        void Move(IPackageLog log, IBottleDestination destination, IEnumerable<BottleReference> references);
    }


    public class BottleMover : IBottleMover
    {
        private readonly IBottleRepository _repository;

        public BottleMover(IBottleRepository repository)
        {
            _repository = repository;
        }

        public void Move(IPackageLog log, IBottleDestination destination, IEnumerable<BottleReference> references)
        {
            var manifests = references.Select(r => 
            { 
                log.Trace("Reading manifest for bottle '{0}'", r.Name);
                return _repository.ReadManifest(r.Name); 
            });

            var explosionRequests = getExplosionRequests(log, manifests, destination);

            foreach (var request in explosionRequests)
            {
                log.Trace("Exploding: '{0}' from '{1}' to '{2}'", request.BottleName, request.BottleDirectory, request.DestinationDirectory);
                _repository.ExplodeFiles(request);
            }
        }

        private IList<BottleExplosionRequest> getExplosionRequests(IPackageLog log, IEnumerable<PackageManifest> manifests, IBottleDestination destination)
        {
            List<BottleExplosionRequest> explosionRequests;
            string currentManifest="<empty>";
            try
            {
                explosionRequests = manifests.SelectMany(m=>
                {
                    currentManifest = m.Name;
                    return destination.DetermineExplosionRequests(m);
                }).ToList();

                log.Trace("Explosion requests: '{0}'", explosionRequests.Count);
            }
            catch (Exception ex)
            {
                throw new Exception("Problem building the explosion requests for '{0}'".ToFormat(currentManifest), ex);
            }
            return explosionRequests;
        }
    }


    // Let's say it already "knows" the physical destination
    public interface IBottleDestination
    {
        IEnumerable<BottleExplosionRequest> DetermineExplosionRequests(PackageManifest manifest);
    }


    public class BottleExplosionRequest
    {
        public BottleExplosionRequest(IPackageLog log)
        {
            Log = log; 
            CopyBehavior = CopyBehavior.overwrite;
        }

        public BottleExplosionRequest()
        {
            Log = new PackageLog();
            CopyBehavior = CopyBehavior.overwrite;
        }

        public string BottleName { get; set; }

        public CopyBehavior CopyBehavior { get; set; }

        /// <summary>
        /// This is the directory within the bottle
        /// </summary>
        public string BottleDirectory { get; set; }

        public string DestinationDirectory { get; set; }

        public IPackageLog Log { get; private set; }

        public bool DetailedLogging { get; set; }

        public bool Equals(BottleExplosionRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.BottleName, BottleName) && Equals(other.BottleDirectory, BottleDirectory) && Equals(other.DestinationDirectory, DestinationDirectory);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(BottleExplosionRequest)) return false;
            return Equals((BottleExplosionRequest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (BottleName != null ? BottleName.GetHashCode() : 0);
                result = (result * 397) ^ (BottleDirectory != null ? BottleDirectory.GetHashCode() : 0);
                result = (result * 397) ^ (DestinationDirectory != null ? DestinationDirectory.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("BottleName: {0}, BottleDirectory: {1}, DestinationDirectory: {2}", BottleName, BottleDirectory, DestinationDirectory);
        }
    }

}