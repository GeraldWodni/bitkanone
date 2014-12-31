\ Cyberfest manifest by John Perry Barlow <barlow@eff.org>

rewind-markup



compiletoflash

: manifesto
s" \r " >m-scroll
s"      Governments of the Industrial World, you weary giants of flesh and steel, I come from Cyberspace, the new home of Mind. " >m-scroll
s"      On behalf of the future, I ask you of the past to leave us alone. You are not welcome among us. You have no sovereignty where we gather. " >m-scroll
s" \g " >m-scroll
s"      We have no elected government, nor are we likely to have one, so I address you with no greater authority than that with which liberty itself always speaks. " >m-scroll
s"      I declare the global social space we are building to be naturally independent of the tyrannies you seek to impose on us. " >m-scroll
s"      You have no moral right to rule us nor do you possess any methods of enforcement we have true reason to fear. " >m-scroll
s" \b " >m-scroll
s"      Governments derive their just powers from the consent of the governed. You have neither solicited nor received ours. " >m-scroll
s"      We did not invite you. " >m-scroll
s"      You do not know us, nor do you know our world. " >m-scroll
s"      Cyberspace does not lie within your borders. " >m-scroll
s"      Do not think that you can build it, as though it were a public construction project. " >m-scroll
s"      You cannot. It is an act of nature and it grows itself through our collective actions. " >m-scroll
s" \y " >m-scroll
s"      You have not engaged in our great and gathering conversation, nor did you create the wealth of our marketplaces. " >m-scroll
s"      You do not know our culture, our ethics, or the unwritten codes that already provide our society more order than could be obtained by any of your impositions. " >m-scroll
s" \m " >m-scroll
s"      You claim there are problems among us that you need to solve. " >m-scroll
s"      You use this claim as an excuse to invade our precincts. " >m-scroll
s"      Many of these problems don't exist. " >m-scroll
s"      Where there are real conflicts, where there are wrongs, we will identify them and address them by our means. " >m-scroll
s"      We are forming our own Social Contract. " >m-scroll
s"      This governance will arise according to the conditions of our world, not yours. Our world is different. " >m-scroll
s" \c " >m-scroll
s"      Cyberspace consists of transactions, relationships, and thought itself, arrayed like a standing wave in the web of our communications. " >m-scroll
s"      Ours is a world that is both everywhere and nowhere, but it is not where bodies live. " >m-scroll
s" \r " >m-scroll
s"      We are creating a world that all may enter without privilege or prejudice accorded by race, economic power, military force, or station of birth. " >m-scroll
s" \g " >m-scroll
s"      We are creating a world where anyone, anywhere may express his or her beliefs, no matter how singular, without fear of being coerced into silence or conformity. " >m-scroll
s" \b " >m-scroll
s"      Your legal concepts of property, expression, identity, movement, and context do not apply to us. " >m-scroll
s"      They are all based on matter, and there is no matter here. " >m-scroll
s" \y " >m-scroll
s"      Our identities have no bodies, so, unlike you, we cannot obtain order by physical coercion. " >m-scroll
s"      We believe that from ethics, enlightened self-interest, and the commonweal, our governance will emerge. " >m-scroll
s"      Our identities may be distributed across many of your jurisdictions. " >m-scroll
s"      The only law that all our constituent cultures would generally recognize is the Golden Rule. " >m-scroll
s"      We hope we will be able to build our particular solutions on that basis. But we cannot accept the solutions you are attempting to impose. " >m-scroll
s" \m " >m-scroll
s"      In the United States, you have today created a law, the Telecommunications Reform Act, which repudiates your own Constitution and " >m-scroll
s"      insults the dreams of Jefferson, Washington, Mill, Madison, DeToqueville, and Brandeis. These dreams must now be born anew in us. " >m-scroll
s" \c " >m-scroll
s"      You are terrified of your own children, since they are natives in a world where you will always be immigrants. " >m-scroll
s"      Because you fear them, you entrust your bureaucracies with the parental responsibilities you are too cowardly to confront yourselves. " >m-scroll
s"      In our world, all the sentiments and expressions of humanity, from the debasing to the angelic, are parts of a seamless whole, " >m-scroll
s"      the global conversation of bits. We cannot separate the air that chokes from the air upon which wings beat. " >m-scroll
s" \r " >m-scroll
s"      In China, Germany, France, Russia, Singapore, Italy and the United States, " >m-scroll
s"      you are trying to ward off the virus of liberty by erecting guard posts at the frontiers of Cyberspace. " >m-scroll
s"      These may keep out the contagion for a small time, " >m-scroll
s"      but they will not work in a world that will soon be blanketed in bit-bearing media. " >m-scroll
s" \g " >m-scroll
s"      Your increasingly obsolete information industries would perpetuate themselves by proposing laws, " >m-scroll
s"      in America and elsewhere, that claim to own speech itself throughout the world. " >m-scroll
s"      These laws would declare ideas to be another industrial product, no more noble than pig iron. " >m-scroll
s"      In our world, whatever the human mind may create can be reproduced and distributed infinitely at no cost. " >m-scroll
s"      The global conveyance of thought no longer requires your factories to accomplish. " >m-scroll
s" \b " >m-scroll
s"      These increasingly hostile and colonial measures place us in the same position as those previous lovers of freedom and self-determination " >m-scroll
s"      who had to reject the authorities of distant, uninformed powers. We must declare our virtual selves immune to your sovereignty, " >m-scroll
s"      even as we continue to consent to your rule over our bodies. We will spread ourselves across the Planet so that no one can arrest our thoughts. " >m-scroll
s" \y " >m-scroll
s"      We will create a civilization of the Mind in Cyberspace. May it be more humane and fair than the world your governments have made before. " >m-scroll
;

: init
	init
	1000 ms
	\ ['] mz-flush flush-target !
	off $00FF00 >rgb 
	5000 ms
	key? invert if
		begin
			manifesto
		key? until
	then ;

