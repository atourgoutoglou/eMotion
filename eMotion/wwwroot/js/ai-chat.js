function toggleAiChat() {
    const panel = document.getElementById("aiChatPanel");

    if (!panel) return;

    panel.classList.toggle("open");
}

function handleAiEnter(event) {
    if (event.key === "Enter") {
        sendAiMessage();
    }
}

function quickAiQuestion(text) {
    const input = document.getElementById("aiChatInput");

    if (!input) return;

    input.value = text;
    sendAiMessage();
}

function sendAiMessage() {

    const input = document.getElementById("aiChatInput");
    const messages = document.getElementById("aiChatMessages");

    if (!input || !messages) return;

    const userText = input.value.trim();

    if (userText.length === 0) return;

    addAiMessage(userText, "user");

    input.value = "";

    showTypingIndicator();

    setTimeout(() => {

        removeTypingIndicator();

        const reply = getAiReply(userText);

        addAiMessage(reply, "bot");

    }, 800);
}

function addAiMessage(text, type) {

    const messages = document.getElementById("aiChatMessages");

    if (!messages) return;

    const message = document.createElement("div");

    message.className = `ai-message ${type}`;

    message.innerText = text;

    messages.appendChild(message);

    messages.scrollTop = messages.scrollHeight;
}

function showTypingIndicator() {

    const messages = document.getElementById("aiChatMessages");

    if (!messages) return;

    const typing = document.createElement("div");

    typing.className = "ai-message bot typing-indicator";

    typing.id = "typingIndicator";

    typing.innerText = "Ο βοηθός πληκτρολογεί...";

    messages.appendChild(typing);

    messages.scrollTop = messages.scrollHeight;
}

function removeTypingIndicator() {

    const typing = document.getElementById("typingIndicator");

    if (typing) {
        typing.remove();
    }
}

function normalizeText(text) {
    return text
        .toLowerCase()
        .replace(/ς/g, "σ")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "");
}

function getAiReply(text) {

    const msg = normalizeText(text);

    if (
        msg.includes("δεν καταλαβα") ||
        msg.includes("δυσκολ") ||
        msg.includes("μπερδε") ||
        msg.includes("δεν μπορω") ||
        msg.includes("βοηθεια")
    ) {

        return "Μην ανησυχείς. Είναι φυσιολογικό να δυσκολεύεσαι σε κάποια σημεία. Προτείνω να επιστρέψεις στη θεωρία του κεφαλαίου και να ξεκινήσεις από τα βασικά βήματα.";
    }

    if (
        msg.includes("αγχοσ") ||
        msg.includes("αγχω") ||
        msg.includes("στρεσ") ||
        msg.includes("φοβα") ||
        msg.includes("πιεση")
    ) {
        return "Καταλαβαίνω ότι μπορεί να νιώθεις άγχος. Κάνε ένα μικρό διάλειμμα, πάρε μερικές βαθιές αναπνοές και συνέχισε με μία άσκηση τη φορά. Η πρόοδος γίνεται σταθερά, όχι βιαστικά.";
    }


    if (
        msg.includes("κουρα") ||
        msg.includes("βαριε") ||
        msg.includes("νυστα") ||
        msg.includes("δεν εχω ορεξη")
    ) {

        return "Φαίνεται ότι ίσως χρειάζεσαι ξεκούραση. Προτείνω ένα σύντομο διάλειμμα 5 λεπτών και μετά επανάληψη με πιο απλές ασκήσεις.";
    }

    if (
        msg.includes("προοδο") ||
        msg.includes("βαθμο") ||
        msg.includes("ολοκληρωση") ||
        msg.includes("score")
    ) {

        return "Μπορείς να δεις αναλυτικά την πρόοδό σου στη σελίδα «Η Πρόοδός μου». Εκεί εμφανίζονται οι βαθμολογίες, οι προσπάθειες και η συνολική ολοκλήρωση.";
    }

    if (
        msg.includes("τι να διαβασω") ||
        msg.includes("που να συνεχισω") ||
        msg.includes("απο που να ξεκινησω")
    ) {

        return "Προτείνω να συνεχίσεις από το κεφάλαιο με τη χαμηλότερη ολοκλήρωση ή από εκεί που δυσκολεύτηκες περισσότερο στο τελευταίο κουίζ.";
    }

    if (
        msg.includes("κεφαλαιο 1") ||
        msg.includes("αναλυση προβληματος")
    ) {

        return "Στο Κεφάλαιο 1 προσπάθησε να ξεχωρίζεις τα δεδομένα, τα ζητούμενα και τα βήματα επίλυσης πριν ξεκινήσεις τη λύση.";
    }


    if (
        msg.includes("κεφαλαιο 2") ||
        msg.includes("αλγοριθμος") ||
        msg.includes("αλγοριθμοι")
    ) {

        return "Στο Κεφάλαιο 2 δώσε έμφαση στα χαρακτηριστικά των αλγορίθμων: σαφήνεια, περατότητα, είσοδος, έξοδος και αποτελεσματικότητα.";
    }

    if (
        msg.includes("κεφαλαιο 3") ||
        msg.includes("γλωσσα") ||
        msg.includes("μεταβλητες")
    ) {

        return "Στο Κεφάλαιο 3 προσπάθησε να κατανοήσεις σωστά τις μεταβλητές, τους τύπους δεδομένων και τη σύνταξη των εντολών.";
    }

    if (
        msg.includes("κεφαλαιο 4") ||
        msg.includes("δομη επιλογης") ||
        msg.includes("δομη επαναληψης") ||
        msg.includes("επαναληψη")
    ) {

        return "Στο Κεφάλαιο 4 δώσε προσοχή στις συνθήκες και στις επαναλήψεις. Ξεκίνα πρώτα με τη λογική του προβλήματος και μετά με τον ψευδοκώδικα.";
    }

    if (
        msg.includes("θεωρια") ||
        msg.includes("βιβλιο") ||
        msg.includes("υλη")
    ) {

        return "Μπορείς να επιστρέψεις στη θεωρία του κεφαλαίου ή να ανοίξεις το «Βιβλίο Μαθητή» για επιπλέον παραδείγματα και επανάληψη.";
    }

    if (
        msg.includes("κουιζ") ||
        msg.includes("ασκηση") ||
        msg.includes("τεστ")
    ) {

        return "Αν δυσκολεύεσαι στις ασκήσεις, προσπάθησε να εντοπίσεις πρώτα τις λέξεις-κλειδιά της εκφώνησης και να χωρίσεις το πρόβλημα σε μικρότερα βήματα.";
    }

    if (
        msg.includes("συναισθημα") ||
        msg.includes("emotion") ||
        msg.includes("πως λειτουργει")
    ) {

        return "Το eMotion αναλύει στοιχεία της μαθησιακής συμπεριφοράς, όπως χρόνο απάντησης, προσπάθειες, αλλαγές απαντήσεων και αλληλεπίδραση με το εκπαιδευτικό υλικό.";
    }

    if (
        msg.includes("γεια") ||
        msg.includes("καλημερα") ||
        msg.includes("καλησπερα") ||
        msg.includes("hello")
    ) {

        return "Γεια σου! Είμαι ο βοηθός μάθησης του eMotion και μπορώ να σε βοηθήσω με θεωρία, κουίζ, πρόοδο και υποστήριξη κατά τη διάρκεια της μελέτης.";
    }


    return "Μπορώ να σε βοηθήσω με τη θεωρία, τα κουίζ, την πρόοδό σου και τη μαθησιακή σου υποστήριξη. Δοκίμασε να γράψεις: «δεν κατάλαβα το κεφάλαιο 1», «έχω άγχος», «τι να διαβάσω τώρα;» ή «πού βλέπω την πρόοδό μου;».";
}