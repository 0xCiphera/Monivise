using Monivise.Application.DTOs.Onboarding;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Services
{
    public interface IAllocationRecommendationService
    {
        IEnumerable<PathwayPreviewDto> BuildPathways(IntakeProfile profile);
    }
}
