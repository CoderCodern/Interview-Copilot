using InterviewCopilot.Domain.Common;
using MediatR;

namespace InterviewCopilot.Application.Common.Messaging;

/// <summary>A command mutates state and returns a <see cref="Result"/> (CQRS write side, Doc 01 §1).</summary>
public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

/// <summary>A query reads state and never mutates it (CQRS read side).</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;

/// <summary>Marker so the UnitOfWork behavior only wraps commands in a transaction.</summary>
public interface ITransactional;
