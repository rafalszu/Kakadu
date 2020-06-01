import { authHeader, apiUrl, handleResponse } from '../_helpers';

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

    return fetch(`${apiUrl}/token/authenticate`, requestOptions)
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
                throw new Error(`Unable to connect to '${apiUrl}'`);
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

    return fetch(`${apiUrl}/users`, requestOptions).then(handleResponse);
}
