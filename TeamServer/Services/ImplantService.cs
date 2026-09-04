using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.APIModels;
using Shared;
using TeamServer.Database;
using TeamServer.Models;
using TeamServer.Service;

namespace TeamServer.Services
{

    [InjectableService]
    public interface IImplantService : IStorable
    {
        Task AddImplantAsync(Implant implant);
        IEnumerable<Implant> GetImplants();
        Implant GetImplant(string id);
        Implant GetImplantbyName(string name);
        Task RemoveImplantAsync(Implant implant);
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

        public async Task AddImplantAsync(Implant implant)
        {
            if (string.IsNullOrEmpty(implant.Name))
                implant.Name = implant.Config.ImplantName;

            if (string.IsNullOrEmpty(implant.Listener) && !string.IsNullOrEmpty(implant.Config.Listener))
                implant.Listener = implant.Config.Listener;

            if (!_implants.ContainsKey(implant.Id))
                _implants.Add(implant.Id, implant);
            else
                _implants[implant.Id] = implant;

            var existingDbImplant = await this._dbService.Get<ImplantDao>(d => d.Id == implant.Id);
            if (existingDbImplant != null)
                await this._dbService.Update((ImplantDao)implant);
            else
                await this._dbService.Insert((ImplantDao)implant);
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

        public async Task RemoveImplantAsync(Implant implant)
        {
            _implants.Remove(implant.Id);
            ImplantDao implantDao = implant;
            implantDao.IsDeleted = true;
            await this._dbService.Update(implantDao);
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
