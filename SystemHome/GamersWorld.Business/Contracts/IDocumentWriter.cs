using GamersWorld.Common.Requests;
using GamersWorld.Common.Responses;

namespace GamersWorld.Business.Contracts;

public interface IDocumentWriter
{
    Task<BusinessResponse> SaveAsync(DocumentSaveRequest payload);
}