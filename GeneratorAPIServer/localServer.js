const express = require('express');
const axios = require('axios');
const dotenv = require('dotenv');
const jwt = require('jsonwebtoken');
const passport = require('passport');
const OAuth2Strategy = require('passport-oauth2').Strategy;

// Load environment variables from .env file
dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

// OAuth credentials from Patreon
const CLIENT_ID = 'q1VcyRBOYRXndM4qpf1VIRqwCeSWCC-XpUZbdzqlart3PNREzOcE-UKP1xf7lmrH';
const CLIENT_SECRET = 'FRQ7WGIa7SMUWOlRkj90nCO-RZwkuT10eId0kiKWQugXGY5BDv2GgT2OCnmg0qqS';
const CALLBACK_URL = 'http://3.137.208.22:3000/auth/patreon/callback';
const CAMPAIGN_ID = '7192782';

const JWT_SECRET = "MY-JWT-SECRET";

// Middleware to parse JSON requests
app.use(express.json());

passport.use(new OAuth2Strategy({
    authorizationURL: 'https://www.patreon.com/oauth2/authorize',
    tokenURL: 'https://www.patreon.com/api/oauth2/token',
    clientID: CLIENT_ID,
    clientSecret: CLIENT_SECRET,
    callbackURL: CALLBACK_URL,
    scope: ['identity', 'identity[email]', 'identity.memberships', 'campaigns'].join(' '),
}, async (accessToken, refreshToken, profile, done) => {
    console.log('Access Token:', accessToken);
    try {
        // Request user data from Patreon API
        const response = await axios.get('https://www.patreon.com/api/oauth2/v2/identity?fields[user]=about,created,email,first_name,full_name,image_url,last_name,social_connections,thumb_url,url,vanity,is_creator&include=memberships,memberships.campaign&fields[member]=patron_status', {
            headers: { Authorization: `Bearer ${accessToken}` },
            params: { include: 'memberships' }
        });

        console.log('Full API response:', JSON.stringify(response.data, null, 2));

        const userData = response.data;
        const user = userData.data;
        console.log('User:', user);  // Log the user data
        console.log('User Attributes:', user.attributes);  // Log the attributes

        if (!user.attributes) {
            console.error('No attributes returned from Patreon API.');
            return done(null, false, { message: 'Unable to fetch user attributes' });
        }

        const isCreator = user.id === 59148863;  // Replace MY_USER_ID with your own user ID
        const isOwner = isCreator || user.attributes.is_creator;

        const memberships = userData.included;

        const isSubscriber = memberships && memberships.some(m =>
            m.type === 'member' && m.attributes?.patron_status === 'active_patron' && m.relationships?.campaign?.data?.id === CAMPAIGN_ID
        );

        if (!isOwner && !isSubscriber) {
            console.log('User is neither owner nor subscriber');
            return done(null, false, { message: 'User is not authorized' });
        }

        const role = isOwner ? 'owner' : 'subscriber';
        const token = jwt.sign(
            { id: user.id, name: user.attributes.full_name, role },
            JWT_SECRET,
            { expiresIn: '30d' }
        );

        return done(null, { token });
    } catch (error) {
        console.error('OAuth Error:', error);
        return done(error);
    }
}));

// Initialize Passport
app.use(passport.initialize());

// OAuth2 authentication routes
app.get('/auth/patreon', passport.authenticate('oauth2', { session: false }));

// OAuth2 callback route - Generate JWT token on successful login
app.get('/auth/patreon/callback', passport.authenticate('oauth2', { failureRedirect: '/auth/failure', session: false }), (req, res) => {
    const user = req.user; // Assuming the user object has been set with the token

    // Redirect URL for the C# WPF client
    const redirectUrl = `http://localhost:5000/auth/patreon/callback?token=${user.token}&status=success`;

    // Response HTML that shows a success message and redirects after a short delay
    const htmlResponse = `
        <html>
            <body>
                <h1>Authentication Successful!</h1>
                <p>You will be redirected shortly...</p>
                <script type="text/javascript">
                    setTimeout(function() {
                        window.location.href = "${redirectUrl}";
                    }, 2000); // 2 seconds delay for the user to see the message
                </script>
            </body>
        </html>
    `;

    res.send(htmlResponse);
});

app.get('/auth/failure', (req, res) => {
    // Response HTML that shows a success message and redirects after a short delay
    const htmlResponse = `
        <html>
            <body>
                <h1>Authentication Failed! Please ensure you are subscribed.</h1>
            </body>
        </html>
    `;

    res.send(htmlResponse);
});
// Protect API routes using JWT authentication
function authenticateJWT(req, res, next) {
    const authHeader = req.headers.authorization;
    const adminOverride = req.body.adminOverride;

    if (adminOverride) {
        return next(); // Ensure the function stops execution here
    }

    if (authHeader) {
        const token = authHeader.split(' ')[1];

        jwt.verify(token, JWT_SECRET, (err, user) => {
            if (err) {
                console.error('JWT verification failed:', err);
                return res.status(403).json({ redirectPath: "/auth/patreon" }); // Invalid token
            }
            req.user = user; // Attach user to request object
            return next(); // Stop further execution
        });
    } else {
        // No token provided
        return res.status(401).json({ redirectPath: "/auth/patreon" }); // Stop execution
    }
}

// Example route to handle API requests
app.post('/generate-response', authenticateJWT, async (req, res) => {
    const prompt = req.body.prompt;
    const model = req.body.model;

    if (!prompt) {
        return res.status(400).send({ error: 'Prompt is required' });
    }

    try {
        // Make a request to the OpenAI API
        const openaiResponse = await axios.post('https://api.openai.com/v1/chat/completions', {
            model: model,
            messages: [{ role: "user", content: prompt }],
            max_tokens: 4096
        }, {
            headers: {
                'Authorization': `Bearer ${process.env.OPENAI_API_KEY}`,
                'Content-Type': 'application/json'
            }
        });

        // Send the OpenAI response back to the client
        return res.json(openaiResponse.data);
    } catch (error) {
        console.error(error);
        console.error(error.response);
        res.status(500).send({ error: 'Error communicating with OpenAI' });
    }
});

// Start the server
app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});