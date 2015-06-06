﻿namespace Merchello.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Lucene.Net.Search;

    using Merchello.Core.Models;
    using Merchello.Core.Models.Interfaces;
    using Merchello.Core.Persistence;
    using Merchello.Core.Persistence.UnitOfWork;
    using Merchello.Core.Persistence.Querying;

    using Umbraco.Core;
    using Umbraco.Core.Events;

    internal class OfferRedeemedService : IOfferRedeemedService
    {
        /// <summary>
        /// The locker.
        /// </summary>
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// The database unit of work provider.
        /// </summary>
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        /// <summary>
        /// The repository factory.
        /// </summary>
        private readonly RepositoryFactory _repositoryFactory;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OfferRedeemedService"/> class.
        /// </summary>
        public OfferRedeemedService()
            : this(new RepositoryFactory())
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OfferRedeemedService"/> class.
        /// </summary>
        /// <param name="repositoryFactory">
        /// The repository factory.
        /// </param>
        public OfferRedeemedService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OfferRedeemedService"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="repositoryFactory">
        /// The repository factory.
        /// </param>
        public OfferRedeemedService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            Mandate.ParameterNotNull(provider, "provider");
            Mandate.ParameterNotNull(repositoryFactory, "repositoryFactory");

            _uowProvider = provider;
            _repositoryFactory = repositoryFactory;
        }

        #endregion


        #region Event Handlers

        /// <summary>
        /// Occurs after Create
        /// </summary>
        public static event TypedEventHandler<IOfferRedeemedService, Events.NewEventArgs<IOfferRedeemed>> Creating;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        public static event TypedEventHandler<IOfferRedeemedService, Events.NewEventArgs<IOfferRedeemed>> Created;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IOfferRedeemedService, SaveEventArgs<IOfferRedeemed>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IOfferRedeemedService, SaveEventArgs<IOfferRedeemed>> Saved;

        /// <summary>
        /// Occurs before Delete
        /// </summary>		
        public static event TypedEventHandler<IOfferRedeemedService, DeleteEventArgs<IOfferRedeemed>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IOfferRedeemedService, DeleteEventArgs<IOfferRedeemed>> Deleted;

        #endregion

        /// <summary>
        /// Creates an <see cref="IOfferRedeemed"/> record
        /// </summary>
        /// <param name="offerSettings">
        /// The offer settings.
        /// </param>
        /// <param name="invoice">
        /// The invoice.
        /// </param>
        /// <param name="raiseEvents">
        /// Optional boolean indicating whether or not to raise events
        /// </param>
        /// <returns>
        /// The <see cref="IOfferRedeemed"/>.
        /// </returns>
        public IOfferRedeemed CreateOfferRedeemedWithKey(IOfferSettings offerSettings, IInvoice invoice, bool raiseEvents = true)
        {
            var redemption = new OfferRedeemed(
                offerSettings.OfferCode,
                offerSettings.OfferProviderKey,
                invoice.Key,
                offerSettings.Key);
            if (raiseEvents)
            if (Creating.IsRaisedEventCancelled(new Events.NewEventArgs<IOfferRedeemed>(redemption), this))
            {
                redemption.WasCancelled = true;
                return redemption;
            }

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(uow))
                {
                    repository.AddOrUpdate(redemption);
                    uow.Commit();
                }
            }

            if (raiseEvents)
            Created.RaiseEvent(new Events.NewEventArgs<IOfferRedeemed>(redemption), this);

            return redemption;
        }

        /// <summary>
        /// Saves an <see cref="IOfferRedeemed"/>
        /// </summary>
        /// <param name="offerRedeemed">
        /// The offer redeemed.
        /// </param>
        /// <param name="raiseEvents">
        /// Optional boolean indicating whether or not to raise events
        /// </param>
        public void Save(IOfferRedeemed offerRedeemed, bool raiseEvents = true)
        {
            if (raiseEvents)
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IOfferRedeemed>(offerRedeemed), this))
            {
                ((OfferRedeemed)offerRedeemed).WasCancelled = true;
                return;
            }

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(uow))
                {
                    repository.AddOrUpdate(offerRedeemed);
                    uow.Commit();
                }
            }

            if (raiseEvents)
            Saved.RaiseEvent(new SaveEventArgs<IOfferRedeemed>(offerRedeemed), this);
        }

        /// <summary>
        /// Saves a collection of <see cref="IOfferRedeemed"/>
        /// </summary>
        /// <param name="redemptions">
        /// The redemptions.
        /// </param>
        /// <param name="raiseEvents">
        /// Optional boolean indicating whether or not to raise events
        /// </param>
        public void Save(IEnumerable<IOfferRedeemed> redemptions, bool raiseEvents = true)
        {
            var redemptionsArray = redemptions as IOfferRedeemed[] ?? redemptions.ToArray();
            if (raiseEvents)
            Saving.RaiseEvent(new SaveEventArgs<IOfferRedeemed>(redemptionsArray), this);

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(uow))
                {
                    foreach (var redemption in redemptionsArray)
                    {
                        repository.AddOrUpdate(redemption);    
                    }
                    
                    uow.Commit();
                }
            }

            if (raiseEvents)
            Saved.RaiseEvent(new SaveEventArgs<IOfferRedeemed>(redemptionsArray), this);
        }

        /// <summary>
        /// Deletes an <see cref="IOfferRedeemed"/>
        /// </summary>
        /// <param name="offerRedeemed">
        /// The offer redeemed.
        /// </param>
        /// <param name="raiseEvents">
        /// Optional boolean indicating whether or not to raise events
        /// </param>
        public void Delete(IOfferRedeemed offerRedeemed, bool raiseEvents = true)
        {
            if (raiseEvents)
            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IOfferRedeemed>(offerRedeemed), this))
            {
                ((OfferRedeemed)offerRedeemed).WasCancelled = true;
                return;
            }


            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(uow))
                {                   
                    repository.Delete(offerRedeemed);                
                    uow.Commit();
                }
            }

            if (raiseEvents) 
            Deleted.RaiseEvent(new DeleteEventArgs<IOfferRedeemed>(offerRedeemed), this);
        }


        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="redemptions">
        /// The redemptions.
        /// </param>
        /// <param name="raiseEvents">
        /// Optional boolean indicating whether or not to raise events
        /// </param>
        public void Delete(IEnumerable<IOfferRedeemed> redemptions, bool raiseEvents = true)
        {
            var redemptionsArray = redemptions as IOfferRedeemed[] ?? redemptions.ToArray();
            
            if (raiseEvents) 
            Deleting.RaiseEvent(new DeleteEventArgs<IOfferRedeemed>(redemptionsArray), this); 

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(uow))
                {
                    foreach (var redemption in redemptionsArray)
                    {
                        repository.Delete(redemption);
                    }

                    uow.Commit();
                }
            }

            if (raiseEvents)
            Deleted.RaiseEvent(new DeleteEventArgs<IOfferRedeemed>(redemptionsArray), this);
        }

        /// <summary>
        /// Gets an <see cref="IOfferRedeemed"/> record by it's key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="IOfferRedeemed"/>.
        /// </returns>
        public IOfferRedeemed GetByKey(Guid key)
        {
            using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(key);
            }
        }

        /// <summary>
        ///  Gets a collection of <see cref="IOfferRedeemed"/> records by an invoice key.
        /// </summary>
        /// <param name="invoiceKey">
        /// The invoice key.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{IOfferRedeemed}"/>.
        /// </returns>
        public IEnumerable<IOfferRedeemed> GetByInvoiceKey(Guid invoiceKey)
        {
            using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IOfferRedeemed>.Builder.Where(x => x.InvoiceKey == invoiceKey);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        ///  Gets a collection of <see cref="IOfferRedeemed"/> records by a customer key.
        /// </summary>
        /// <param name="customerKey">
        /// The customer key.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{IOfferRedeemed}"/>.
        /// </returns>
        public IEnumerable<IOfferRedeemed> GetByCustomerKey(Guid customerKey)
        {
            using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IOfferRedeemed>.Builder.Where(x => x.CustomerKey == customerKey);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IOfferRedeemed"/> records by a offer settings key.
        /// </summary>
        /// <param name="offerSettingsKey">
        /// The offer settings key.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{IOfferRedeemed}"/>.
        /// </returns>
        public IEnumerable<IOfferRedeemed> GetByOfferSettingsKey(Guid offerSettingsKey)
        {
            using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IOfferRedeemed>.Builder.Where(x => x.OfferSettingsKey == offerSettingsKey);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// The get by offer settings key and customer key.
        /// </summary>
        /// <param name="offerSettingsKey">
        /// The offer settings key.
        /// </param>
        /// <param name="customerKey">
        /// The customer key.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{IOfferRedeemed}"/>.
        /// </returns>
        public IEnumerable<IOfferRedeemed> GetByOfferSettingsKeyAndCustomerKey(Guid offerSettingsKey, Guid customerKey)
        {
            using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IOfferRedeemed>.Builder.Where(x => x.OfferSettingsKey == offerSettingsKey && x.CustomerKey == customerKey);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IOfferRedeemed"/> records by an offer provider key.
        /// </summary>
        /// <param name="offerProviderKey">
        /// The offer provider key.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{IOfferRedeemed}"/>.
        /// </returns>
        public IEnumerable<IOfferRedeemed> GetByOfferProviderKey(Guid offerProviderKey)
        {
            using (var repository = _repositoryFactory.CreateOfferRedeemedRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IOfferRedeemed>.Builder.Where(x => x.OfferProviderKey == offerProviderKey);
                return repository.GetByQuery(query);
            }
        }
    }
}