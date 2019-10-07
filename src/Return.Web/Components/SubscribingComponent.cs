﻿// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SubscribingComponent.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using System;
    using Application.Notifications;
    using Microsoft.AspNetCore.Components;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    /// <summary>
    /// Base class for components which subscribe to a single subscription
    /// </summary>
    /// <typeparam name="TSubscription"></typeparam>
    public class SubscribingComponent<TSubscription> : MediatorComponent, ISubscriber where TSubscription : ISubscriber {
        [Inject]
        public INotificationSubscription<TSubscription> Subscription { get; set; }

        public Guid UniqueId { get; } = Guid.NewGuid();

        protected override void OnInitialized() => this.Subscription.Subscribe((TSubscription)(object)this);

        protected void NotificationIsHandled() => this.StateHasChanged();
    }
}
