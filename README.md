LegalLab
==============

Desktop WPF application allowing you to work and experiment with TAG Legal objects: 

* Digital IDs
* Smart Contracts
* e-Daler Wallet
* Contractual & Conditional Payments
* Marketplace Auctions
* Neuro-Features

Example Contracts
----------------------

The repository contains several example contracts that can be used as templates in order to experiment and get to learn how to use 
TAG technologies.

* [Syntax](ExampleContracts/Syntax) contains examples related to the syntax of different aspects of smart contracts.

	* [Syntax.xml](ExampleContracts/Syntax/Markdown.xml) describes the Markdown syntax available for designing human-readable text in
	smart contracts. Sign such a contract to illustrate receipt and understanding of the information contained in the contract.

	* [CalculatedParameters.xml](ExampleContracts/Syntax/CalculatedParameters.xml) shows how calculated parameters can be used in contracts.

* [Marketplace](ExampleContracts/Marketplace) contains Marketplace examples.

	* [Buying Services](ExampleContracts/Marketplace/Buying%20Services) contains examples related to buying services on the marketplace.

		* [BuyService.xml](ExampleContracts/Marketplace/Buying%20Services/BuyService.xml) is a Request for Tenders, where a *Buyer* requests 
		service providers to offer their services to the *Buyer*. The *Aauctioneer* selects the best option, or the first offer that reaches 
		the acceptable level. Bad offers are immediately rejected.

		* [OfferService.xml](ExampleContracts/Marketplace/Buying%20Services/OfferService.xml) represents an offer of a servce by a *Seller*, 
		requested in a Request for Tenders made in a separate contract by a *Buyer*.

	* [Selling Items](ExampleContracts/Marketplace/Selling%20Items) contains examples related to selling items on the marketplace.

		* [SellItem.xml](ExampleContracts/Marketplace/Selling%20Items/SellItem.xml) is a Request for Tenders, where a *Seller* requests 
		prospective buyers to make offer on an item made for sale by the *Seller*. The *Aauctioneer* selects the best option, or the first 
		offer that reaches the acceptable level. Bad offers are immediately rejected.

		* [OfferItem.xml](ExampleContracts/Marketplace/Selling%20Items/OfferItem.xml) represents an offer by a *Buyer* to by an item, 
		as a response to a Request for Tenders made in a separate contract by a *Seller*.

* [Tokens](ExampleContracts/Tokens) contains examples on how to create and manage Neuro-Feature tokens.

	* [Create](ExampleContracts/Tokens/Create) contains examples on how to create tokens.

		* [Specific](ExampleContracts/Tokens/Create/Specific) contains specific examples on how to create tokens.

			* [LimundoItemToken.xml](ExampleContracts/Tokens/Create/Specific/LimundoItemToken.xml) creates a mirror token of an item in Limundo.

			* [LimundoItemToken.xsd](ExampleContracts/Tokens/Create/Specific/LimundoItemToken.xsd) contains the schema for the definition of
			a Limundo token.

		* [CreateTokenContract1.xml](ExampleContracts/Tokens/Create/CreateTokenContract1.xml) can be used to create one named (ID) token.

		* [CreateTokenContract1RandomId.xml](ExampleContracts/Tokens/Create/CreateTokenContract1RandomId.xml) can be used to create one token,
		where the Trust Provider assigns a random ID.
		
		* [CreateTokenContract1UniqueId.xml](ExampleContracts/Tokens/Create/CreateTokenContract1UniqueId.xml) can be used to create one token whose
		ID is determined by its contents, and is therefore guaranteed to be unique.
		
		* [CreateTokenContract5.xml](ExampleContracts/Tokens/Create/CreateTokenContract5.xml) can be used to create a batch of five named (ID) tokens.

		* [CreateTokenContract5RandomId.xml](ExampleContracts/Tokens/Create/CreateTokenContract5RandomId.xml) can be used to create a batch
		of five tokens, where the Trust Provider assigns them random IDs.
		
		* [CreateTokenContract5UniqueId.xml](ExampleContracts/Tokens/Create/CreateTokenContract5UniqueId.xml) can be used to create a batch
		of five tokens, whose IDs are determined by their contents, and are therefore guaranteed to be unique, as a batch.

	* [Destroy](ExampleContracts/Tokens/Destroy) contains examples on how to destroy tokens.

	* [Transfer](ExampleContracts/Tokens/Transfer) contains examples on how to transfer the ownership of tokens.