using Microsoft.EntityFrameworkCore;
using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.Data
{
    public class SislocDbContext : DbContext
    {
        public SislocDbContext(DbContextOptions<SislocDbContext> options) : base(options)
        {
        }

        // DbSets - Representam as tabelas do banco
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Motorista> Motoristas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações das tabelas
            modelBuilder.Entity<Agendamento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Protocolo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.NomeSolicitante).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Destino).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.ObservacoesAdmin).HasMaxLength(1000);

                // Configuração dos relacionamentos
                entity.HasOne(a => a.VeiculoAlocado)
                      .WithMany(v => v.Agendamentos)
                      .HasForeignKey(a => a.VeiculoAlocadoId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.MotoristaAlocado)
                      .WithMany(m => m.Agendamentos)
                      .HasForeignKey(a => a.MotoristaAlocadoId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Índices para performance
                entity.HasIndex(e => e.Protocolo).IsUnique();
                entity.HasIndex(e => e.DataPartida);
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Placa).IsRequired().HasMaxLength(8);
                entity.Property(e => e.Modelo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Observacoes).HasMaxLength(500);

                // Índices
                entity.HasIndex(e => e.Placa).IsUnique();
                entity.HasIndex(e => e.Categoria);
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<Motorista>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NomeCompleto).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NumeroCnh).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CategoriaCnh).IsRequired().HasMaxLength(5);
                entity.Property(e => e.Telefone).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Observacoes).HasMaxLength(500);

                // Índices
                entity.HasIndex(e => e.NumeroCnh).IsUnique();
                entity.HasIndex(e => e.Status);
            });

            // Seed Data - Dados iniciais para desenvolvimento
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Veículos
            modelBuilder.Entity<Veiculo>().HasData(
                new Veiculo
                {
                    Id = 1,
                    Placa = "ABC-1234",
                    Modelo = "Gol 1.0",
                    Categoria = CategoriaVeiculo.Hatch,
                    CapacidadePassageiros = 5,
                    Status = StatusVeiculo.Disponivel
                },
                new Veiculo
                {
                    Id = 2,
                    Placa = "DEF-5678",
                    Modelo = "Corolla XEI",
                    Categoria = CategoriaVeiculo.Sedan,
                    CapacidadePassageiros = 5,
                    Status = StatusVeiculo.Disponivel
                },
                new Veiculo
                {
                    Id = 3,
                    Placa = "GHI-9012",
                    Modelo = "Hilux SR",
                    Categoria = CategoriaVeiculo.Pickup,
                    CapacidadePassageiros = 5,
                    Status = StatusVeiculo.Disponivel
                }
            );

            // Seed Motoristas
            modelBuilder.Entity<Motorista>().HasData(
                new Motorista
                {
                    Id = 1,
                    NomeCompleto = "João Silva Santos",
                    NumeroCnh = "12345678901",
                    VencimentoCnh = new DateTime(2026, 12, 31), // Data fixa
                    CategoriaCnh = "B",
                    Telefone = "(11) 99999-9999",
                    DataExameToxicologico = new DateTime(2024, 6, 15), // Data fixa
                    Status = StatusMotorista.Disponivel
                },
                new Motorista
                {
                    Id = 2,
                    NomeCompleto = "Maria Oliveira Costa",
                    NumeroCnh = "98765432109",
                    VencimentoCnh = new DateTime(2025, 12, 31), // Data fixa
                    CategoriaCnh = "C",
                    Telefone = "(11) 88888-8888",
                    DataExameToxicologico = new DateTime(2024, 9, 15), // Data fixa
                    Status = StatusMotorista.Disponivel
                }
            );
        }
    }
}