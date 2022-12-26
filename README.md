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

* [Marketplace](ExampleContracts/Marketplace) contains Marketplace examples.

	* [Buying Services](ExampleContracts/Marketplace/Buying) contains examples related to buying services on the marketplace.

		* [BuyService.xml](ExampleContracts/Marketplace/Buying/BuyService.xml) is a Request for Tenders, where a *Buyer* requests 
		service providers to offer their services to the *Buyer*. The *Aauctioneer* selects the best option, or the first offer that reaches 
		the acceptable level. Bad offers are immediately rejected.

		* [OfferService.xml](ExampleContracts/Marketplace/Buying/OfferService.xml) represents an offer of a servce by a *Seller*, 
		requested in a Request for Tenders made in a separate contract by a *Buyer*.

	* [Selling Items](ExampleContracts/Marketplace/Selling) contains examples related to selling items on the marketplace.

		* [SellItem.xml](ExampleContracts/Marketplace/Selling/SellItem.xml) is a Request for Tenders, where a *Seller* requests 
		prospective buyers to make offer on an item made for sale by the *Seller*. The *Aauctioneer* selects the best option, or the first 
		offer that reaches the acceptable level. Bad offers are immediately rejected.

		* [SellToken.xml](ExampleContracts/Marketplace/Selling/SellToken.xml) is a Request for Tenders, where a *Seller* requests 
		prospective buyers to make offer on a token made for sale by the *Seller*. The *Aauctioneer* selects the best option, or the first 
		offer that reaches the acceptable level. Bad offers are immediately rejected.

		* [OfferItem.xml](ExampleContracts/Marketplace/Selling/OfferItem.xml) represents an offer by a *Buyer* to by an item
		(or token), as a response to a Request for Tenders made in a separate contract by a *Seller*.

* [StateMachines](ExampleContracts/StateMachines) contains examples related to creating state-machine-based tokens.

	* [Calculator.xml](ExampleContracts/StateMachines/Calculator.xml) creates a simple calculator state-machine, that can be controlled
	by local and external text and XML notes.

	* [CrowdFunding.xml](ExampleContracts/StateMachines/CrowdFunding.xml) creates a state-machine for crowd-funding a project. The
	state-machine also contains an escrow feature that allows the machine to control how funds are released to the project, for the
	duration of the project.

	* [MicroLoan.xml](ExampleContracts/StateMachines/MicroLoan.xml) creates a state-machine implementing a micro-loan, where a borrower
	borrows a small amount of money from a lender for a small amount of time. The state-machine automatically returns the borrowed money
	according to an agreed plan. Failed attempts are logged, and interest accumulated up to a maximum debt limit.

	* [ProjectEscrow.xml](ExampleContracts/StateMachines/ProjectEscrow.xml) creates a state-machine for an escrow account for a project.
	The buyer of the token invests into the project, and the escrow feature allows the machine to control how funds are released to the 
	project, for the duration of the project.

* [Syntax](ExampleContracts/Syntax) contains examples related to the syntax of different aspects of smart contracts.

	* [Syntax.xml](ExampleContracts/Syntax/Markdown.xml) describes the Markdown syntax available for designing human-readable text in
	smart contracts. Sign such a contract to illustrate receipt and understanding of the information contained in the contract.

	* [CalculatedParameters.xml](ExampleContracts/Syntax/CalculatedParameters.xml) shows how calculated parameters can be used in contracts.

* [Tokens](ExampleContracts/Tokens) contains examples on how to create and manage Neuro-Feature tokens.

	* [Create](ExampleContracts/Tokens/Create) contains examples on how to create tokens.

		* [Specific](ExampleContracts/Tokens/Create/Specific) contains specific examples on how to create tokens.

			* [Achievement.xml](ExampleContracts/Tokens/Create/Specific/Achievement.xml) creates a unique achievement token
			that can be awarded to a recipient for special occations.

			* [DemoToken.xml](ExampleContracts/Tokens/Create/Specific/DemoToken.xml) creates a simple token with a friendly name, that can
			be used for demo.

		* [CreateTokenContract1.xml](ExampleContracts/Tokens/Create/CreateTokenContract1.xml) can be used to create one named (ID) token.
		The operation includes instant payments of commissions.

		* [CreateTokenContract1RandomId.xml](ExampleContracts/Tokens/Create/CreateTokenContract1RandomId.xml) can be used to create one token,
		where the Trust Provider assigns a random ID. The operation includes instant payments of commissions.
		
		* [CreateTokenContract1UniqueId.xml](ExampleContracts/Tokens/Create/CreateTokenContract1UniqueId.xml) can be used to create one token whose
		ID is determined by its contents, and is therefore guaranteed to be unique. The operation includes instant payments of commissions.
		
		* [CreateTokenContract5.xml](ExampleContracts/Tokens/Create/CreateTokenContract5.xml) can be used to create a batch of five named (ID) 
		tokens. The operation includes instant payments of commissions.

		* [CreateTokenContract5RandomId.xml](ExampleContracts/Tokens/Create/CreateTokenContract5RandomId.xml) can be used to create a batch
		of five tokens, where the Trust Provider assigns them random IDs. The operation includes instant payments of commissions.
		
		* [CreateTokenContract5UniqueId.xml](ExampleContracts/Tokens/Create/CreateTokenContract5UniqueId.xml) can be used to create a batch
		of five tokens, whose IDs are determined by their contents, and are therefore guaranteed to be unique, as a batch. The operation 
		includes instant payments of commissions.

	* [Destroy](ExampleContracts/Tokens/Destroy) contains examples on how to destroy tokens.
	
		* [DestroyTokenContract1.xml](ExampleContracts/Tokens/Destroy/DestroyTokenContract1.xml) can be used to destroy a token one owns.
	
		* [DestroyTokenContract5.xml](ExampleContracts/Tokens/Destroy/DestroyTokenContract5.xml) can be used to destroy a batch of five tokens,
		as long as one owns all tokens in the batch.

	* [Transfer](ExampleContracts/Tokens/Transfer) contains examples on how to transfer the ownership of tokens.
	
		* [TransferTokenContract1.xml](ExampleContracts/Tokens/Transfer/TransferTokenContract1.xml) can be used to transfer the ownership of a
		token from the current owner to a new owner. The process includes assigning a new value, as well as performing instant payments.
	
		* [TransferTokenContract5.xml](ExampleContracts/Tokens/Transfer/TransferTokenContract5.xml) can be used to transfer the ownership of a
		batch of five tokens from the current owner to a new owner. The process includes assigning a new value for the tokens, as well as 
		performing instant payments.
