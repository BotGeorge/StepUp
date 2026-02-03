using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepUp.Application.DTOs.ActivityLog;
using StepUp.Application.DTOs.Auth;
using StepUp.Application.DTOs.Challenge;
using StepUp.Application.DTOs.Participation;
using StepUp.Application.DTOs.User;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;
using StepUp.Infrastructure.Data;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : BaseController
{
    private readonly IUserService _userService;
    private readonly IChallengeService _challengeService;
    private readonly IParticipationService _participationService;
    private readonly IActivityService _activityService;
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SeedController(
        IUserService userService,
        IChallengeService challengeService,
        IParticipationService participationService,
        IActivityService activityService,
        IAuthService authService,
        ApplicationDbContext context,
        IMapper mapper)
    {
        _userService = userService;
        _challengeService = challengeService;
        _participationService = participationService;
        _activityService = activityService;
        _authService = authService;
        _context = context;
        _mapper = mapper;
    }

    [HttpDelete("cleanup")]
    public async Task<ActionResult> CleanupData(CancellationToken cancellationToken)
    {
        try
        {
            // Delete all data in correct order (respecting foreign keys)
            _context.ActivityLogs.RemoveRange(_context.ActivityLogs);
            _context.Participations.RemoveRange(_context.Participations);
            _context.FriendRequests.RemoveRange(_context.FriendRequests);
            _context.Challenges.RemoveRange(_context.Challenges);
            _context.Users.RemoveRange(_context.Users);
            
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Datele au fost șterse cu succes!"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la ștergerea datelor.",
                error = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult> SeedData(CancellationToken cancellationToken)
    {
        try
        {
            // 1. Creează utilizatori cu parole
            var userAccounts = new List<object>();

            // User 1 - Alex Popescu
            var user1Password = "Alex123!";
            var user1 = await _authService.RegisterAsync(new RegisterDto
            {
                Name = "Alex Popescu",
                Email = "alex.popescu@example.com",
                Password = user1Password
            }, cancellationToken);
            userAccounts.Add(new { Name = user1.Name, Email = user1.Email, Password = user1Password, Role = "User" });

            // User 2 - Maria Ionescu
            var user2Password = "Maria123!";
            var user2 = await _authService.RegisterAsync(new RegisterDto
            {
                Name = "Maria Ionescu",
                Email = "maria.ionescu@example.com",
                Password = user2Password
            }, cancellationToken);
            userAccounts.Add(new { Name = user2.Name, Email = user2.Email, Password = user2Password, Role = "User" });

            // User 3 - Andrei Georgescu
            var user3Password = "Andrei123!";
            var user3 = await _authService.RegisterAsync(new RegisterDto
            {
                Name = "Andrei Georgescu",
                Email = "andrei.georgescu@example.com",
                Password = user3Password
            }, cancellationToken);
            userAccounts.Add(new { Name = user3.Name, Email = user3.Email, Password = user3Password, Role = "User" });

            // User 4 - Admin
            UserDto? admin = null;
            var adminPassword = "Admin123!";
            try
            {
                admin = await _authService.RegisterAsync(new RegisterDto
                {
                    Name = "Admin User",
                    Email = "admin@stepup.com",
                    Password = adminPassword
                }, cancellationToken);
                
                // Update admin role
                var adminEntity = await _context.Users.FindAsync(new object[] { admin.Id }, cancellationToken);
                if (adminEntity != null)
                {
                    adminEntity.Role = Role.Admin;
                    await _context.SaveChangesAsync(cancellationToken);
                }
                userAccounts.Add(new { Name = admin.Name, Email = admin.Email, Password = adminPassword, Role = "Admin" });
            }
            catch (InvalidOperationException)
            {
                // User already exists, try to get it and update role
                var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@stepup.com", cancellationToken);
                if (existingAdmin != null)
                {
                    existingAdmin.Role = Role.Admin;
                    await _context.SaveChangesAsync(cancellationToken);
                    admin = _mapper.Map<UserDto>(existingAdmin);
                    userAccounts.Add(new { Name = admin.Name, Email = admin.Email, Password = adminPassword, Role = "Admin" });
                }
            }

            // User 5 - Test User
            UserDto? testUser = null;
            var testPassword = "Test123!";
            try
            {
                testUser = await _authService.RegisterAsync(new RegisterDto
                {
                    Name = "Test User",
                    Email = "test@stepup.com",
                    Password = testPassword
                }, cancellationToken);
                userAccounts.Add(new { Name = testUser.Name, Email = testUser.Email, Password = testPassword, Role = "User" });
            }
            catch (InvalidOperationException)
            {
                // User already exists, get it
                var existingTestUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@stepup.com", cancellationToken);
                if (existingTestUser != null)
                {
                    testUser = _mapper.Map<UserDto>(existingTestUser);
                    userAccounts.Add(new { Name = testUser.Name, Email = testUser.Email, Password = testPassword, Role = "User" });
                }
            }

            // User 6 - Partner
            UserDto? partner = null;
            var partnerPassword = "Partner123!";
            try
            {
                partner = await _authService.RegisterAsync(new RegisterDto
                {
                    Name = "Fitness Partner",
                    Email = "partner@stepup.com",
                    Password = partnerPassword
                }, cancellationToken);
                
                // Update partner role
                var partnerEntity = await _context.Users.FindAsync(new object[] { partner.Id }, cancellationToken);
                if (partnerEntity != null)
                {
                    partnerEntity.Role = Role.Partner;
                    await _context.SaveChangesAsync(cancellationToken);
                }
                userAccounts.Add(new { Name = partner.Name, Email = partner.Email, Password = partnerPassword, Role = "Partner" });
            }
            catch (InvalidOperationException)
            {
                // User already exists, try to get it and update role
                var existingPartner = await _context.Users.FirstOrDefaultAsync(u => u.Email == "partner@stepup.com", cancellationToken);
                if (existingPartner != null)
                {
                    existingPartner.Role = Role.Partner;
                    await _context.SaveChangesAsync(cancellationToken);
                    partner = _mapper.Map<UserDto>(existingPartner);
                    userAccounts.Add(new { Name = partner.Name, Email = partner.Email, Password = partnerPassword, Role = "Partner" });
                }
            }

            // 2. Creează challenge-uri pentru fiecare categorie
            
            var challenges = new List<ChallengeDto>();

            // STEPS - Pași
            // Challenge activ - Steps (cu target)
            var stepsActiveStart = DateTime.UtcNow.AddDays(-5);
            var stepsActiveEnd = DateTime.UtcNow.AddDays(25);
            var stepsActive = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "10K Steps Daily Challenge",
                MetricType = MetricType.Steps,
                StartDate = stepsActiveStart,
                EndDate = stepsActiveEnd,
                TargetValue = 10000
            }, userId: null, cancellationToken);
            challenges.Add(stepsActive);

            // Challenge viitor - Steps (endless)
            var stepsFutureStart = DateTime.UtcNow.AddDays(7);
            var stepsFutureEnd = DateTime.UtcNow.AddDays(37);
            var stepsFuture = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "15K Steps Challenge",
                MetricType = MetricType.Steps,
                StartDate = stepsFutureStart,
                EndDate = stepsFutureEnd,
                TargetValue = null // Endless
            }, userId: null, cancellationToken);
            challenges.Add(stepsFuture);

            // Challenge terminat - Steps (cu target)
            var stepsCompletedStart = DateTime.UtcNow.AddDays(-40);
            var stepsCompletedEnd = DateTime.UtcNow.AddDays(-10);
            var stepsCompleted = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "8K Steps Marathon",
                MetricType = MetricType.Steps,
                StartDate = stepsCompletedStart,
                EndDate = stepsCompletedEnd,
                TargetValue = 8000
            }, userId: null, cancellationToken);
            challenges.Add(stepsCompleted);

            // RUNNING - Alergare
            // Challenge activ - Running (cu target)
            var runningActiveStart = DateTime.UtcNow.AddDays(-3);
            var runningActiveEnd = DateTime.UtcNow.AddDays(27);
            var runningActive = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "5K Running Challenge",
                MetricType = MetricType.Running,
                StartDate = runningActiveStart,
                EndDate = runningActiveEnd,
                TargetValue = 5 // 5 km
            }, userId: null, cancellationToken);
            challenges.Add(runningActive);

            // Challenge viitor - Running (endless)
            var runningFutureStart = DateTime.UtcNow.AddDays(10);
            var runningFutureEnd = DateTime.UtcNow.AddDays(40);
            var runningFuture = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "10K Marathon Training",
                MetricType = MetricType.Running,
                StartDate = runningFutureStart,
                EndDate = runningFutureEnd,
                TargetValue = null // Endless
            }, userId: null, cancellationToken);
            challenges.Add(runningFuture);

            // Challenge terminat - Running (cu target)
            var runningCompletedStart = DateTime.UtcNow.AddDays(-35);
            var runningCompletedEnd = DateTime.UtcNow.AddDays(-5);
            var runningCompleted = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "3K Sprint Challenge",
                MetricType = MetricType.Running,
                StartDate = runningCompletedStart,
                EndDate = runningCompletedEnd,
                TargetValue = 3 // 3 km
            }, userId: null, cancellationToken);
            challenges.Add(runningCompleted);

            // PHYSICAL EXERCISES - Exerciții Fizice
            // Challenge activ - PhysicalExercises (cu target și tip)
            var exercisesActiveStart = DateTime.UtcNow.AddDays(-7);
            var exercisesActiveEnd = DateTime.UtcNow.AddDays(23);
            var exercisesActive = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "100 Flotări Challenge",
                MetricType = MetricType.PhysicalExercises,
                StartDate = exercisesActiveStart,
                EndDate = exercisesActiveEnd,
                TargetValue = 100,
                ExerciseType = "Flotări"
            }, userId: null, cancellationToken);
            challenges.Add(exercisesActive);

            // Challenge viitor - PhysicalExercises (cu target și tip)
            var exercisesFutureStart = DateTime.UtcNow.AddDays(14);
            var exercisesFutureEnd = DateTime.UtcNow.AddDays(44);
            var exercisesFuture = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "200 Abdomene Challenge",
                MetricType = MetricType.PhysicalExercises,
                StartDate = exercisesFutureStart,
                EndDate = exercisesFutureEnd,
                TargetValue = 200,
                ExerciseType = "Abdomene"
            }, userId: null, cancellationToken);
            challenges.Add(exercisesFuture);

            // Challenge terminat - PhysicalExercises (endless)
            var exercisesCompletedStart = DateTime.UtcNow.AddDays(-30);
            var exercisesCompletedEnd = DateTime.UtcNow.AddDays(-1);
            var exercisesCompleted = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "50 Flotări Quick Challenge",
                MetricType = MetricType.PhysicalExercises,
                StartDate = exercisesCompletedStart,
                EndDate = exercisesCompletedEnd,
                TargetValue = 50,
                ExerciseType = "Flotări"
            }, userId: null, cancellationToken);
            challenges.Add(exercisesCompleted);

            // CALORIE BURN - CalorieBurn
            // Challenge activ - CalorieBurn (cu target)
            var caloriesActiveStart = DateTime.UtcNow.AddDays(-2);
            var caloriesActiveEnd = DateTime.UtcNow.AddDays(28);
            var caloriesActive = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "500 Calorie Burn Daily",
                MetricType = MetricType.CalorieBurn,
                StartDate = caloriesActiveStart,
                EndDate = caloriesActiveEnd,
                TargetValue = 500
            }, userId: null, cancellationToken);
            challenges.Add(caloriesActive);

            // Challenge viitor - CalorieBurn (endless)
            var caloriesFutureStart = DateTime.UtcNow.AddDays(5);
            var caloriesFutureEnd = DateTime.UtcNow.AddDays(35);
            var caloriesFuture = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "1000 Calorie Burn Challenge",
                MetricType = MetricType.CalorieBurn,
                StartDate = caloriesFutureStart,
                EndDate = caloriesFutureEnd,
                TargetValue = null // Endless
            }, userId: null, cancellationToken);
            challenges.Add(caloriesFuture);

            // Challenge terminat - CalorieBurn (cu target)
            var caloriesCompletedStart = DateTime.UtcNow.AddDays(-25);
            var caloriesCompletedEnd = DateTime.UtcNow.AddDays(-2);
            var caloriesCompleted = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
            {
                Name = "300 Calorie Quick Burn",
                MetricType = MetricType.CalorieBurn,
                StartDate = caloriesCompletedStart,
                EndDate = caloriesCompletedEnd,
                TargetValue = 300
            }, userId: null, cancellationToken);
            challenges.Add(caloriesCompleted);

            // SPONSORED CHALLENGE - Challenge sponsorizat cu premiu
            var sponsoredStart = DateTime.UtcNow.AddDays(-1);
            var sponsoredEnd = DateTime.UtcNow.AddDays(29);
            ChallengeDto? sponsoredChallenge = null;
            if (partner != null)
            {
                try
                {
                    sponsoredChallenge = await _challengeService.CreateChallengeAsync(new CreateChallengeDto
                    {
                        Name = "🏆 Sponsored: 10K Steps pentru Voucher 500 RON",
                        MetricType = MetricType.Steps,
                        StartDate = sponsoredStart,
                        EndDate = sponsoredEnd,
                        TargetValue = null, // Endless
                        Description = "Participă la acest challenge sponsorizat și câștigă un voucher de 500 RON! Cel mai bun scor câștigă premiul.",
                        IsSponsored = true,
                        Prize = "Voucher 500 RON",
                        SponsorId = null // Va fi setat automat de ChallengeService
                    }, partner.Id, cancellationToken);
                    challenges.Add(sponsoredChallenge);
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    Console.WriteLine($"Error creating sponsored challenge: {ex.Message}");
                }
            }

            // 3. Înscrie userii la challenge-urile active
            foreach (var challenge in challenges)
            {
                var now = DateTime.UtcNow;
                var isActive = challenge.StartDate <= now && challenge.EndDate >= now;
                var isUpcoming = challenge.StartDate > now;

                if (isActive || isUpcoming)
                {
                    // Înscrie userii la challenge-uri active sau viitoare
                    try
                    {
                        await _participationService.JoinChallengeAsync(new JoinChallengeDto
                        {
                            UserId = user1.Id,
                            ChallengeId = challenge.Id
                        }, cancellationToken);
                    }
                    catch { }

                    try
                    {
                        await _participationService.JoinChallengeAsync(new JoinChallengeDto
                        {
                            UserId = user2.Id,
                            ChallengeId = challenge.Id
                        }, cancellationToken);
                    }
                    catch { }

                    // Doar user1 și user2 la unele challenge-uri pentru varietate
                    if (challenge.MetricType == MetricType.Steps && isActive)
                    {
                        try
                        {
                            await _participationService.JoinChallengeAsync(new JoinChallengeDto
                            {
                                UserId = user3.Id,
                                ChallengeId = challenge.Id
                            }, cancellationToken);
                        }
                        catch { }
                    }
                }
            }

            // Înscrie participanți la challenge-ul sponsorizat
            if (sponsoredChallenge != null)
            {
                // Înscrie user1, user2, user3 la challenge-ul sponsorizat
                try
                {
                    await _participationService.JoinChallengeAsync(new JoinChallengeDto
                    {
                        UserId = user1.Id,
                        ChallengeId = sponsoredChallenge.Id
                    }, cancellationToken);
                }
                catch { }

                try
                {
                    await _participationService.JoinChallengeAsync(new JoinChallengeDto
                    {
                        UserId = user2.Id,
                        ChallengeId = sponsoredChallenge.Id
                    }, cancellationToken);
                }
                catch { }

                try
                {
                    await _participationService.JoinChallengeAsync(new JoinChallengeDto
                    {
                        UserId = user3.Id,
                        ChallengeId = sponsoredChallenge.Id
                    }, cancellationToken);
                }
                catch { }

                // Adaugă activity logs pentru challenge-ul sponsorizat
                var sponsoredChallengeStart = sponsoredChallenge.StartDate;
                
                // User 1: 12000, 11500, 13000 (total: 36500) - cel mai bun
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user1.Id,
                    MetricValue = 12000,
                    Date = sponsoredChallengeStart.AddDays(1)
                }, cancellationToken);
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user1.Id,
                    MetricValue = 11500,
                    Date = sponsoredChallengeStart.AddDays(2)
                }, cancellationToken);
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user1.Id,
                    MetricValue = 13000,
                    Date = DateTime.UtcNow.AddDays(-1)
                }, cancellationToken);

                // User 2: 10000, 10500, 11000 (total: 31500) - al doilea
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user2.Id,
                    MetricValue = 10000,
                    Date = sponsoredChallengeStart.AddDays(1)
                }, cancellationToken);
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user2.Id,
                    MetricValue = 10500,
                    Date = sponsoredChallengeStart.AddDays(2)
                }, cancellationToken);
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user2.Id,
                    MetricValue = 11000,
                    Date = DateTime.UtcNow.AddDays(-1)
                }, cancellationToken);

                // User 3: 9000, 9500, 9200 (total: 27700) - al treilea
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user3.Id,
                    MetricValue = 9000,
                    Date = sponsoredChallengeStart.AddDays(1)
                }, cancellationToken);
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user3.Id,
                    MetricValue = 9500,
                    Date = sponsoredChallengeStart.AddDays(2)
                }, cancellationToken);
                await _activityService.AddActivityLogAsync(new CreateActivityLogDto
                {
                    UserId = user3.Id,
                    MetricValue = 9200,
                    Date = DateTime.UtcNow.AddDays(-1)
                }, cancellationToken);

                // Calculează scorurile pentru challenge-ul sponsorizat
                await _challengeService.CalculateAndUpdateScoresAsync(sponsoredChallenge.Id, cancellationToken);
            }

            // 4. Adaugă activity logs pentru challenge-ul activ de Steps (pentru leaderboard)
            var stepsChallenge = challenges.First(c => c.Name == "10K Steps Daily Challenge");
            var challengeStart = stepsChallenge.StartDate;
            
            // User 1: 8500, 9200, 10500, 8800, 11000 (total: 48000)
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user1.Id,
                MetricValue = 8500,
                Date = challengeStart.AddDays(1)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user1.Id,
                MetricValue = 9200,
                Date = challengeStart.AddDays(2)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user1.Id,
                MetricValue = 10500,
                Date = challengeStart.AddDays(3)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user1.Id,
                MetricValue = 8800,
                Date = challengeStart.AddDays(4)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user1.Id,
                MetricValue = 11000,
                Date = DateTime.UtcNow.AddDays(-1)
            }, cancellationToken);

            // User 2: 12000, 11500, 13000, 12500, 12800 (total: 61800) - cel mai bun
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user2.Id,
                MetricValue = 12000,
                Date = challengeStart.AddDays(1)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user2.Id,
                MetricValue = 11500,
                Date = challengeStart.AddDays(2)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user2.Id,
                MetricValue = 13000,
                Date = challengeStart.AddDays(3)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user2.Id,
                MetricValue = 12500,
                Date = challengeStart.AddDays(4)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user2.Id,
                MetricValue = 12800,
                Date = DateTime.UtcNow.AddDays(-1)
            }, cancellationToken);

            // User 3: 7500, 8000, 8200, 7800, 7900 (total: 39400) - cel mai slab
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user3.Id,
                MetricValue = 7500,
                Date = challengeStart.AddDays(1)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user3.Id,
                MetricValue = 8000,
                Date = challengeStart.AddDays(2)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user3.Id,
                MetricValue = 8200,
                Date = challengeStart.AddDays(3)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user3.Id,
                MetricValue = 7800,
                Date = challengeStart.AddDays(4)
            }, cancellationToken);
            await _activityService.AddActivityLogAsync(new CreateActivityLogDto
            {
                UserId = user3.Id,
                MetricValue = 7900,
                Date = DateTime.UtcNow.AddDays(-1)
            }, cancellationToken);

            // 5. Calculează și actualizează scorurile pentru challenge-ul activ
            await _challengeService.CalculateAndUpdateScoresAsync(stepsChallenge.Id, cancellationToken);

            // 6. Setează LastActiveAt pentru statusuri online/offline diferite
            var user1Entity = await _context.Users.FindAsync(new object[] { user1.Id }, cancellationToken);
            var user2Entity = await _context.Users.FindAsync(new object[] { user2.Id }, cancellationToken);
            var user3Entity = await _context.Users.FindAsync(new object[] { user3.Id }, cancellationToken);

            if (user1Entity != null)
            {
                // User1: Online (activ în ultimele 2 minute)
                user1Entity.LastActiveAt = DateTime.UtcNow.AddMinutes(-2);
            }

            if (user2Entity != null)
            {
                // User2: Offline (activ acum 10 minute - peste pragul de 5 minute)
                user2Entity.LastActiveAt = DateTime.UtcNow.AddMinutes(-10);
            }

            if (user3Entity != null)
            {
                // User3: Online (activ în ultimele 1 minut)
                user3Entity.LastActiveAt = DateTime.UtcNow.AddMinutes(-1);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // 7. Creează prietenie între user1 și user2 (2 prieteni)
            if (user1Entity != null && user2Entity != null)
            {
                // Creează FriendRequest Accepted în ambele direcții (pentru relație bidirecțională)
                var friendRequest1 = new FriendRequest
                {
                    Id = Guid.NewGuid(),
                    FromUserId = user1.Id,
                    ToUserId = user2.Id,
                    Status = FriendRequestStatus.Accepted,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                };

                var friendRequest2 = new FriendRequest
                {
                    Id = Guid.NewGuid(),
                    FromUserId = user2.Id,
                    ToUserId = user1.Id,
                    Status = FriendRequestStatus.Accepted,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                };

                _context.FriendRequests.Add(friendRequest1);
                _context.FriendRequests.Add(friendRequest2);
            }

            // 8. Creează cerere pending de la user3 către user1
            if (user3Entity != null && user1Entity != null)
            {
                var pendingRequest = new FriendRequest
                {
                    Id = Guid.NewGuid(),
                    FromUserId = user3.Id,
                    ToUserId = user1.Id,
                    Status = FriendRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                };

                _context.FriendRequests.Add(pendingRequest);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Seed data a fost creată cu succes!",
                data = new
                {
                    userAccounts = userAccounts,
                    users = new[] { user1, user2, user3, admin, testUser, partner },
                    challenges = challenges,
                    totalChallenges = challenges.Count,
                    sponsoredChallenge = sponsoredChallenge != null ? new { 
                        id = sponsoredChallenge.Id, 
                        name = sponsoredChallenge.Name,
                        prize = sponsoredChallenge.Prize,
                        leaderboardUrl = $"/api/challenges/{sponsoredChallenge.Id}/leaderboard"
                    } : null,
                    leaderboardUrl = $"/api/challenges/{stepsChallenge.Id}/leaderboard"
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la crearea seed data.",
                error = ex.Message
            });
        }
    }
}

