import { authHeader } from '../_helpers';

export const userService = {
    login,
    logout,
    getAll
};

function login(username, password) {
    const requestOptions = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
    };

    return fetch(`${process.env.REACT_APP_API_URL}/token/authenticate`, requestOptions)
        //.then(checkFetch)
        .then(handleResponse)
        .then(user => {
            if (user.token) {
                localStorage.setItem('user', JSON.stringify(user));
            }

            return user;
        })
        .catch(error => {
            console.log(error);
            if(!error.response) {
                throw new Error(`Unable to connect to '${process.env.REACT_APP_API_URL}'`);
            }
            throw Error(error.response);
        });
}

function logout() {
    localStorage.removeItem('user');
}

function getAll() {
    const requestOptions = {
        method: 'GET',
        headers: authHeader()
    };

    return fetch(`${process.env.REACT_APP_API_URL}/users`, requestOptions).then(handleResponse);
}

function handleResponse(response) {
    return response.text().then(text => {
        const data = text && JSON.parse(text);
        if (!response.ok) {
            
            const error = { 
                response: (data && data.Message) || response.url + ' - ' + response.statusText
            };
            return Promise.reject(error);
        }

        return data;
    });
}