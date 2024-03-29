﻿using Alpha.Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Alpha.Identity.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<AlphaUser>(options)
{
}
