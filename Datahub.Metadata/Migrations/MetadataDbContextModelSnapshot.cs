﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Datahub.Metadata.Model;

namespace Datahub.Metadata.Migrations
{
    [DbContext(typeof(MetadataDbContext))]
    partial class MetadataDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.9")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Datahub.Metadata.Model.ApprovalForm", b =>
                {
                    b.Property<int>("ApprovalFormId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Approval_InSitu_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Approval_Other_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Approval_Other_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Authority_To_Release_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Branch_NAME")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<bool>("Can_Be_Released_For_Free_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Collection_Of_Datasets_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Copyright_Restrictions_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Dataset_Title_TXT")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Department_NAME")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Division_NAME")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Email_EMAIL")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("Localized_Metadata_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Machine_Readable_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Name_NAME")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("Non_Propietary_Format_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Not_Clasified_Or_Protected_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Phone_TXT")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<bool>("Private_Personal_Information_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Requires_Blanket_Approval_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Section_NAME")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Sector_NAME")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<bool>("Subject_To_Exceptions_Or_Eclusions_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Type_Of_Data_TXT")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("nvarchar(16)");

                    b.Property<bool>("Updated_On_Going_Basis_FLAG")
                        .HasColumnType("bit");

                    b.HasKey("ApprovalFormId");

                    b.ToTable("ApprovalForms");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.FieldChoice", b =>
                {
                    b.Property<int>("FieldChoiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FieldDefinitionId")
                        .HasColumnType("int");

                    b.Property<string>("Label_English_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Label_French_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value_TXT")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("FieldChoiceId");

                    b.HasIndex("FieldDefinitionId");

                    b.ToTable("FieldChoices");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.FieldDefinition", b =>
                {
                    b.Property<int>("FieldDefinitionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Custom_Field_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("English_DESC")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Field_Name_TXT")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("French_DESC")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MetadataVersionId")
                        .HasColumnType("int");

                    b.Property<bool>("MultiSelect_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Name_English_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name_French_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Required_FLAG")
                        .HasColumnType("bit");

                    b.Property<int>("Sort_Order_NUM")
                        .HasColumnType("int");

                    b.Property<string>("Validators_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("FieldDefinitionId");

                    b.HasIndex("MetadataVersionId");

                    b.HasIndex("Field_Name_TXT", "MetadataVersionId")
                        .IsUnique();

                    b.ToTable("FieldDefinitions");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.Keyword", b =>
                {
                    b.Property<int>("KeywordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("English_TXT")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("French_TXT")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("Frequency")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("KeywordId");

                    b.HasIndex("English_TXT")
                        .IsUnique()
                        .HasFilter("[English_TXT] IS NOT NULL");

                    b.HasIndex("French_TXT")
                        .IsUnique()
                        .HasFilter("[French_TXT] IS NOT NULL");

                    b.ToTable("Keywords");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.MetadataVersion", b =>
                {
                    b.Property<int>("MetadataVersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Last_Update_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Source_TXT")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Version_Info_TXT")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("MetadataVersionId");

                    b.ToTable("MetadataVersions");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.ObjectFieldValue", b =>
                {
                    b.Property<long>("ObjectMetadataId")
                        .HasColumnType("bigint");

                    b.Property<int>("FieldDefinitionId")
                        .HasColumnType("int");

                    b.Property<string>("Value_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ObjectMetadataId", "FieldDefinitionId");

                    b.HasIndex("FieldDefinitionId");

                    b.ToTable("ObjectFieldValues");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.ObjectMetadata", b =>
                {
                    b.Property<long>("ObjectMetadataId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("MetadataVersionId")
                        .HasColumnType("int");

                    b.Property<string>("ObjectId_TXT")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("ObjectMetadataId");

                    b.HasIndex("MetadataVersionId");

                    b.HasIndex("ObjectId_TXT")
                        .IsUnique();

                    b.ToTable("ObjectMetadata");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.SubSubject", b =>
                {
                    b.Property<int>("SubSubjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name_English_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name_French_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SubSubjectId");

                    b.ToTable("SubSubjects");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.Subject", b =>
                {
                    b.Property<int>("SubjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Subject_TXT")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("SubjectId");

                    b.HasIndex("Subject_TXT")
                        .IsUnique()
                        .HasFilter("[Subject_TXT] IS NOT NULL");

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("SubSubjectSubject", b =>
                {
                    b.Property<int>("SubSubjectsSubSubjectId")
                        .HasColumnType("int");

                    b.Property<int>("SubjectsSubjectId")
                        .HasColumnType("int");

                    b.HasKey("SubSubjectsSubSubjectId", "SubjectsSubjectId");

                    b.HasIndex("SubjectsSubjectId");

                    b.ToTable("SubSubjectSubject");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.FieldChoice", b =>
                {
                    b.HasOne("Datahub.Metadata.Model.FieldDefinition", "FieldDefinition")
                        .WithMany("Choices")
                        .HasForeignKey("FieldDefinitionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FieldDefinition");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.FieldDefinition", b =>
                {
                    b.HasOne("Datahub.Metadata.Model.MetadataVersion", "MetadataVersion")
                        .WithMany("Definitions")
                        .HasForeignKey("MetadataVersionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MetadataVersion");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.ObjectFieldValue", b =>
                {
                    b.HasOne("Datahub.Metadata.Model.FieldDefinition", "FieldDefinition")
                        .WithMany("FieldValues")
                        .HasForeignKey("FieldDefinitionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Datahub.Metadata.Model.ObjectMetadata", "ObjectMetadata")
                        .WithMany("FieldValues")
                        .HasForeignKey("ObjectMetadataId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FieldDefinition");

                    b.Navigation("ObjectMetadata");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.ObjectMetadata", b =>
                {
                    b.HasOne("Datahub.Metadata.Model.MetadataVersion", "MetadataVersion")
                        .WithMany("Objects")
                        .HasForeignKey("MetadataVersionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MetadataVersion");
                });

            modelBuilder.Entity("SubSubjectSubject", b =>
                {
                    b.HasOne("NRCan.Datahub.Metadata.Model.SubSubject", null)
                        .WithMany()
                        .HasForeignKey("SubSubjectsSubSubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NRCan.Datahub.Metadata.Model.Subject", null)
                        .WithMany()
                        .HasForeignKey("SubjectsSubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Datahub.Metadata.Model.FieldDefinition", b =>
                {
                    b.Navigation("Choices");

                    b.Navigation("FieldValues");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.MetadataVersion", b =>
                {
                    b.Navigation("Definitions");

                    b.Navigation("Objects");
                });

            modelBuilder.Entity("Datahub.Metadata.Model.ObjectMetadata", b =>
                {
                    b.Navigation("FieldValues");
                });
#pragma warning restore 612, 618
        }
    }
}
