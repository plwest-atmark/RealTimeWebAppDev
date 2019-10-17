using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Tests.TestUtilities
{
    public class FakeSession : ISession
    {
        #region Not Implemented for this Chapter

        // automatic implementation for the ISession. We will not be using this so we will not change the default exception thrown
        public bool IsAvailable => throw new NotImplementedException();
        // automatic implementation for the ISession. We will not be using this so we will not change the default exception thrown
        public string Id => throw new NotImplementedException();
        // automatic implementation for the ISession. We will not be using this so we will not change the default exception thrown
        public IEnumerable<string> Keys => throw new NotImplementedException();

        // automatic implementation for the ISession. We will not be using this so we will not change the default exception thrown
        public void Clear()
        {
            throw new NotImplementedException();
        }

        // automatic implementation for the ISession. We will not be using this so we will not change the default exception thrown
        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        // automatic implementation for the ISession. We will not be using this so we will not change the default exception thrown
        public Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        // automatic implementation for the ISession. We will not be using this so we will not change the default exception thrown
        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        #endregion

        // the session factory for ISession. This is where we will be storing session data. Note that we can store
        // many pieces of session information using different keys and byte[] arrays.
        private Dictionary<string, byte[]> sessionFactory = new Dictionary<string, byte[]>();

        /// <summary>
        /// This method is used to store the information in our session dictionary.
        /// 
        /// It checks to ensure that there isn't the same kind of key (so we don't throw an exception)
        /// and then adds the new data if it's not already present.
        /// 
        /// If the key is already present, it will update the information for that key in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, byte[] value)
        {
            if (!sessionFactory.ContainsKey(key))
                sessionFactory.Add(key, value);
            else
                sessionFactory[key] = value;
        }


        /// <summary>
        /// This method is used to retreive the information in our session dictionary.
        /// 
        /// 
        /// It checks to ensure that the key provided is present, and if it is, retrieves the
        /// information from the dictionary.
        /// 
        /// If the information is now present, it will return a null value. This will have to be managed
        /// by the consumer of the session.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out byte[] value)
        {
            if (sessionFactory.ContainsKey(key) && sessionFactory[key] != null)
            {
                value = sessionFactory[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
