﻿// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospective.Commands.JoinRetrospective {
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.ValueObjects;
    using MediatR;
    using Notifications;
    using Queries.GetParticipantsInfo;
    using Return.Common;
    using Services;

    public sealed class JoinRetrospectiveCommandHandler : IRequestHandler<JoinRetrospectiveCommand> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public JoinRetrospectiveCommandHandler(IReturnDbContext returnDbContext, ICurrentParticipantService currentParticipantService, IMediator mediator, IMapper mapper) {
            this._returnDbContext = returnDbContext;
            this._currentParticipantService = currentParticipantService;
            this._mediator = mediator;
            this._mapper = mapper;
        }

        public async Task<Unit> Handle(JoinRetrospectiveCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Retrospective retrospective = await this._returnDbContext.Retrospectives.FindByRetroId(request.RetroId, cancellationToken).ConfigureAwait(false);

            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.RetroId);
            }

            var participant = new Participant {
                IsManager = request.JoiningAsManager,
                Name = request.Name,
                Retrospective = retrospective,
                Color = new ParticipantColor {
                    R = Byte.Parse(request.Color[0..2], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                    G = Byte.Parse(request.Color[2..4], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                    B = Byte.Parse(request.Color[4..6], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                }
            };

            this._returnDbContext.Participants.Add(participant);
            await this._returnDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            this._currentParticipantService.SetParticipant(participant.Id, participant.Name, request.JoiningAsManager);

            await this._mediator.Publish(
                new RetrospectiveJoinedNotification(this._mapper.Map<ParticipantInfo>(participant)), cancellationToken)
                .ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
