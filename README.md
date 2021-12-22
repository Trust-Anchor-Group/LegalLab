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

* [Markdown](ExampleContracts/Markdown) contains examples related to use of Markdown when designing smart contracts

	* [Syntax.xml](ExampleContracts/Markdown/Syntax.xml) describes the Markdown syntax available for designing human-readable text in
	smart contracts. Sign such a contract to illustrate receipt and understanding of the information contained in the contract.

* [Marketplace](ExampleContracts/Marketplace) contains Marketplace examples

	* [BuyService.xml](ExampleContracts/Marketplace/BuyService.xml) is a Request for Tenders, where a *Buyer* requests service providers to offer their 
	services to the *Buyer*. The *Aauctioneer* selects the best option, or the first offer that reaches the acceptable level. Bad offers are immediately
	rejected.

	* [OfferService.xml](ExampleContracts/Marketplace/OfferService.xml) represents an offer of a servce by a *Seller*, requested in a Request for Tenders
	made in a separate contract by a *Buyer*.