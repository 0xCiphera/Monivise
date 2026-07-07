using Monivise.Application.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Services
{
    public interface ISurplusSweepService
    {
        Task<WeeklyReviewDto> BuildReviewAsync(Guid userId, CancellationToken ct = default);
        Task ApplySweepAsync(Guid userId, ApplySweepDto dto, CancellationToken ct = default);
    }
}
