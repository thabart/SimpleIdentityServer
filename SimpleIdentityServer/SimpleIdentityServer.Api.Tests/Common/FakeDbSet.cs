using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class FakeDbSet<TEntity> : IDbSet<TEntity> where TEntity : class
    {
        private readonly List<TEntity> _entities;

        public FakeDbSet()
        {
            _entities = new List<TEntity>();
        }

        public Type ElementType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        public Expression Expression
        {
            get
            {
                return _entities.AsQueryable().Expression;
            }
        }

        public ObservableCollection<TEntity> Local
        {
            get
            {
                return null;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return _entities.AsQueryable().Provider;
            }
        }

        public TEntity Add(TEntity entity)
        {
            _entities.Add(entity);
            return entity;
        }

        public TEntity Attach(TEntity entity)
        {
            return entity;
        }

        public TEntity Create()
        {
            return null;
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            return null;
        }

        public TEntity Find(params object[] keyValues)
        {
            return null;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        public TEntity Remove(TEntity entity)
        {
            _entities.Remove(entity);
            return entity;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entities.GetEnumerator();
        }
    }
}