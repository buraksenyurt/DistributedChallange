﻿using GamersWorld.Common.Requests;

namespace GamersWorld.Repository;

public interface IDocumentRepository
{
    Task<int> InsertDocumentAsync(DocumentSaveRequest documentSaveRequest);
    Task<byte[]> ReadDocumentAsync(DocumentReadRequest documentReadRequest);
}
