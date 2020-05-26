import { authHeader, apiUrl } from '../_helpers';

export const serviceService = {
    getAll,
    getByCode,
    update
};

const requestOptions = {
    method: 'GET',
    headers: authHeader()
};

async function getAll() {
    const response = await fetch(`${apiUrl}/service`, requestOptions);
    return handleResponse(response);
}

async function getByCode(code) {
    const response = await fetch(`${apiUrl}/service/${code}`, requestOptions);
    return handleResponse(response);
}

async function update(serviceCode, data) {
    const response = await fetch(`${apiUrl}/service/${serviceCode}`, {
        method: 'PATCH',
        headers: Object.assign(authHeader(), {
            'Content-Type': 'application/json',
        }),
        body: JSON.stringify(data)
    });

    return handleResponse(response);
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