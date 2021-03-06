using System;
using System.Collections.Immutable;
using System.Linq;

using ReduxWannabe.Core;
using ReduxWannabe.Examples.TodoApp.Actions;
using ReduxWannabe.Examples.TodoApp.State;

namespace ReduxWannabe.Examples.TodoApp
{
    public static class Reducers
    {
        public static ApplicationState ReduceApplication(ApplicationState previousState, IAction action)
        {
            var filter = action is FilterTodosAction filterAction ?
                filterAction.Filter : previousState.Filter;
            var allTodos = TodosReducer(previousState.AllTodos, action);
            var completeAllIsChecked = allTodos.All(x => x.IsCompleted);
            var completeAllIsVisible = allTodos.Any();
            var clearTodosIsVisible = allTodos.Any(todo => todo.IsCompleted);
            var areFiltersVisible = allTodos.Any();
            var count = allTodos.Count(todo => !todo.IsCompleted);
            var items = count == 1 ? "item" : "items";
            var activeTodosCounterMessage = $"{count} {items} left";
            var filteredTodos = filter != TodosFilter.All ?
                allTodos.Where(t => t.IsCompleted == (filter == TodosFilter.Completed)) :
                allTodos;

            return new ApplicationState(
                allTodos: allTodos,
                filter: filter,
                filteredTodos: filteredTodos,
                completeAllIsChecked: completeAllIsChecked,
                completeAllIsVisible: completeAllIsVisible,
                clearTodosIsVisible: clearTodosIsVisible,
                activeTodosCounterMessage: activeTodosCounterMessage,
                areFiltersVisible: areFiltersVisible);
        }

        public static ImmutableArray<Todo> TodosReducer(ImmutableArray<Todo> previousState, IAction action)
        {
            switch (action)
            {
                case AddTodoAction addTodo:
                    return TodoCollectionReducers.AddTodoReducer(previousState, addTodo);
                case ClearCompletedTodosAction clearCompletedTodos:
                    return TodoCollectionReducers.ClearCompletedTodosReducer(previousState, clearCompletedTodos);
                case CompleteAllTodosAction completeAllTodos:
                    return TodoCollectionReducers.CompleteAllTodosReducer(previousState, completeAllTodos);
                case CompleteTodoAction completeTodo:
                    return TodoCollectionReducers.CompleteTodoReducer(previousState, completeTodo);
                case DeleteTodoAction deleteTodo:
                    return TodoCollectionReducers.DeleteTodoReducer(previousState, deleteTodo);
                default:
                    return previousState;
            }
        }

        public static class TodoCollectionReducers
        {
            public static ImmutableArray<Todo> AddTodoReducer(ImmutableArray<Todo> previousState, AddTodoAction action)
            {
                return previousState.Insert(0, new Todo(
                    text: action.Text,
                    isCompleted: false,
                    id : Guid.NewGuid()
                ));
            }

            public static ImmutableArray<Todo> ClearCompletedTodosReducer(ImmutableArray<Todo> previousState, ClearCompletedTodosAction action)
            {
                return previousState.RemoveAll(todo => todo.IsCompleted);
            }

            public static ImmutableArray<Todo> CompleteAllTodosReducer(ImmutableArray<Todo> previousState, CompleteAllTodosAction action)
            {
                return previousState.Select(x => new Todo(
                    text: x.Text,
                    isCompleted: action.IsCompleted,
                    id: x.Id
                )).ToImmutableArray();
            }

            public static ImmutableArray<Todo> CompleteTodoReducer(ImmutableArray<Todo> previousState, CompleteTodoAction action)
            {
                var todoToEdit = previousState.First(todo => todo.Id == action.TodoId);
                return previousState.Replace(todoToEdit, new Todo(
                    id: todoToEdit.Id,
                    text: todoToEdit.Text,
                    isCompleted: !todoToEdit.IsCompleted
                ));
            }

            public static ImmutableArray<Todo> DeleteTodoReducer(ImmutableArray<Todo> previousState, DeleteTodoAction action)
            {
                var todoToDelete = previousState.First(todo => todo.Id == action.TodoId);
                return previousState.Remove(todoToDelete);
            }
        }
    }
}
