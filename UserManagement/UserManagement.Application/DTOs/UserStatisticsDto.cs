using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Application.DTOs
{
    public class UserStatisticsDto
    {
        public StatusStatisticsDto StatusStats { get; set; }
        public RoleStatisticsDto RoleStats { get; set; }
    }

    public class StatusStatisticsDto
    {
        public int Active { get; set; }
        public int Inactive { get; set; }
        public double ActivePercentage { get; set; }
        public double InactivePercentage { get; set; }
    }

    public class RoleStatisticsDto
    {
        public int Admin { get; set; }
        public int User { get; set; }
        public double AdminPercentage { get; set; }
        public double UserPercentage { get; set; }
    }
}