using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.APIModels;
using Shared;
using TeamServer.Database;
using TeamServer.Models;
using TeamServer.Models.Implant;
using TeamServer.Service;

namespace TeamServer.Services
{

    [InjectableService]
    public interface IImplantService : IStorable
    {
        void AddImplant(Implant implant);
        IEnumerable<Implant> GetImplants();
        Implant GetImplant(string id);
        Implant GetImplantbyName(string name);
        void RemoveImplant(Implant implant);
    }

    [InjectableServiceImplementation(typeof(IImplantService))]
    public class ImplantService : IImplantService
    {
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IDatabaseService _dbService;
        public ImplantService(IChangeTrackingService changeTrackingService, IDatabaseService dbService)
        {
            _changeTrackingService = changeTrackingService;
            _dbService = dbService;
        }

        private readonly Dictionary<string, Implant> _implants = new();

        public void AddImplant(Implant implant)
        {
            if (string.IsNullOrEmpty(implant.Name))
                implant.Name = implant.Config.ImplantName;

            if (string.IsNullOrEmpty(implant.Listener) && !string.IsNullOrEmpty(implant.Config.Listener))
                implant.Listener = implant.Config.Listener;

            if (!_implants.ContainsKey(implant.Id))
                _implants.Add(implant.Id, implant);
            else
                _implants[implant.Id] = implant;

            var existingDbImplant = this._dbService.Get<ImplantDao>(d => d.Id == implant.Id).Result;
            if (existingDbImplant != null)
                this._dbService.Update((ImplantDao)implant).Wait();
            else
                this._dbService.Insert((ImplantDao)implant).Wait();
        }

        public Implant GetImplant(string id)
        {
            if (!_implants.ContainsKey(id))
                return null;
            return _implants[id];
        }

        public Implant GetImplantbyName(string name)
        {
            foreach (var implant in this._implants.Values)
            {
                if(string.IsNullOrEmpty(implant.Name)) continue;
                if (implant.Name.ToLower() == name.ToLower())
                    return implant;
            }
            return null;
        }

        public IEnumerable<Implant> GetImplants()
        {
            return _implants.Values;
        }

        public void RemoveImplant(Implant implant)
        {
            _implants.Remove(implant.Id);
            ImplantDao implantDao = implant;
            implantDao.IsDeleted = true;
            this._dbService.Update(implantDao).Wait();
        }


        public async Task LoadFromDB()
        {
            this._implants.Clear();
            var implants = await this._dbService.Load<ImplantDao>();
            foreach (var implant in implants)
            {
                if (implant.IsDeleted)
                    continue;

                this._implants.Add(implant.Id, implant);
            }

        }
    }
}
