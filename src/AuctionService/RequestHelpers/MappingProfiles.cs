using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
    {
        public MappingProfiles ( )
            {
                CreateMap< Auction, AuctionDto > ( ).IncludeMembers ( x => x.Item );
                CreateMap< Item, AuctionDto > ( );

                CreateMap< CreateAuctionDto, Auction > ( ).ForMember ( d => d.Item, o => o.MapFrom ( s => s ) );

                CreateMap< CreateAuctionDto, Item > ( );

                /**********************************************************************************
                 *                                                                                *
                 *   This line of code is part of the AutoMapper configuration in the             *
                 *   'MappingProfiles' class and is used for mapping between the 'AuctionDto'     *
                 *   Data Transfer Object (DTO) and the 'AuctionCreated' Contract.                *
                 *                                                                                *
                 *   AutoMapper is a simple little library built to solve a deceptively complex   *
                 *   problem - getting rid of code that mapped one object to another. This type   *
                 *   of code is rather boring and tedious to write, so why not invent a tool to   *
                 *   do it for us?                                                                *
                 *                                                                                *
                 *   In this context, the code is configuring the AutoMapper to understand how    *
                 *   to convert or map an 'AuctionDto' object to an 'AuctionCreated' object.      *
                 *   This is useful when the system needs to create an 'AuctionCreated' event     *
                 *   with the data coming as 'AuctionDto'.                                        *
                 *                                                                                *
                 **********************************************************************************/
                CreateMap< AuctionDto, AuctionCreated > ( );
                CreateMap< Auction, AuctionUpdated > ( ).IncludeMembers ( a => a.Item );
                CreateMap< Item, AuctionUpdated > ( );
                CreateMap< AuctionDto, AuctionDeleted > ( );
            }
    }