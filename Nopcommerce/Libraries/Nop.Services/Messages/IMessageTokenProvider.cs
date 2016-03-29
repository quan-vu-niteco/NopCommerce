using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Stores;

namespace Nop.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        void AddStoreTokens(IList<Token> tokens, Store store, EmailAccount emailAccount);
       

        void AddCustomerTokens(IList<Token> tokens, Customer customer);

        void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription);

        void AddNewsCommentTokens(IList<Token> tokens, NewsComment newsComment);

        string[] GetListOfCampaignAllowedTokens();

        string[] GetListOfAllowedTokens();
    }
}
